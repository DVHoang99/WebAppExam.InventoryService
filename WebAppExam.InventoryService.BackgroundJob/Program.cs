using Hangfire;
using Hangfire.Redis.StackExchange;
using KafkaFlow;
using KafkaFlow.Serializer;
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

// Configure Redis connection for Hangfire
var redisConfig = builder.Configuration.GetSection("Redis")["Configuration"] ?? DatabaseSettings.DefaultRedisConnection;

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseRedisStorage(redisConfig, new RedisStorageOptions
    {
        Prefix = "hangfire:inventory:",
        Db = 0
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
        "inventory-outbox",
        job => job.ProcessOutboxMessagesAsync(),
        "*/5 * * * * *");
}

await host.RunAsync();
