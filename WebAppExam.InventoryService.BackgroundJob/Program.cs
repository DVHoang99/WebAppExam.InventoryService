using Hangfire;
using Hangfire.Mongo;
using Hangfire.Mongo.Migration.Strategies;
using Hangfire.Mongo.Migration.Strategies.Backup;
using Hangfire.States;
using KafkaFlow;
using KafkaFlow.Serializer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Infrastructure;
using WebAppExam.InventoryService.Infrastructure.Constants;
using WebAppExam.InventoryService.Infrastructure.Common;
using WebAppExam.InventoryService.BackgroundJob.Jobs;

var builder = Host.CreateApplicationBuilder(args);

// Add services from Infrastructure and Application
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Overide Job registration to use local implementation
builder.Services.AddScoped<IOutboxPublisherJob, OutboxPublisherJob>();

// Re-configure Hangfire Server in this service
var connectionString = builder.Configuration.GetSection("MongoDbSettings")["ConnectionString"];
var databaseName = builder.Configuration.GetSection("MongoDbSettings")["DatabaseName"];

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseMongoStorage(connectionString, databaseName, new MongoStorageOptions
    {
        MigrationOptions = new MongoMigrationOptions
        {
            MigrationStrategy = new MigrateMongoMigrationStrategy(),
            BackupStrategy = new CollectionMongoBackupStrategy()
        },
        Prefix = "hangfire",
        CheckConnection = true
    })
);

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 1;
    options.Queues = new[] { "inventory-outbox" };
});

// Configure Kafka Flow (Needed for producers in Outbox Job)
var kafkaBrokers = builder.Configuration.GetSection("KafkaConfig:Brokers").Get<string[]>()
                   ?? new[] { KafkaSettings.DefaultBroker };

builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(kafkaBrokers)
        .AddProducer(
            KafkaProducers.OrderReplyProducer,
            producer => producer
                .DefaultTopic(KafkaTopics.OrderReplyTopic)
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer, MessageTypeResolver>())
        )
        .AddProducer(
            KafkaProducers.OrderUpdatedReplyProducer,
            producer => producer
                .DefaultTopic(KafkaTopics.OrderUpdatedReplyTopic)
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer, MessageTypeResolver>())
        )
        .AddProducer(
            KafkaProducers.OrderCanceledReplyProducer,
            producer => producer
                .DefaultTopic(KafkaTopics.OrderCanceledReplyTopic)
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer, MessageTypeResolver>())
        )
    )
);

var host = builder.Build();

// Setup Kafka Bus
var kafkaBus = host.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

// Schedule Recurring Job
using (var scope = host.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
    recurringJobManager.AddOrUpdate<IOutboxPublisherJob>(
        "outbox-publisher",
        job => job.ProcessOutboxMessagesAsync(),
        "*/5 * * * * *",
        new RecurringJobOptions
        {
            QueueName = "inventory-outbox"
        });
}

await host.RunAsync();
