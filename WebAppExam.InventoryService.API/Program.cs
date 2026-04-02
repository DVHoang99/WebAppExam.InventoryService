
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Infrastructure.Repositories;
using KafkaFlow;
using KafkaFlow.Serializer;
using WebAppExam.InventoryService.Infrastructure.Consumers;
using WebAppExam.InventoryService.Infrastructure.Services;
using StackExchange.Redis;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;

var builder = WebApplication.CreateBuilder(args);
var kafkaBrokers = builder.Configuration.GetSection("KafkaConfig:Brokers").Get<string[]>()
                   ?? new[] { "localhost:9092" };

builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(kafkaBrokers)
        .AddProducer(
            "order-reply",
            producer => producer
                .DefaultTopic("order-reply-topic")
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )
        .AddProducer(
            "order-updated-reply",
            producer => producer
                .DefaultTopic("order-updated-reply-topic")
                .AddMiddlewares(m => m.AddSerializer<JsonCoreSerializer>())
        )

        .AddConsumer(consumer => consumer
            .Topic("order-created-topic")
            .WithGroupId("inventory-consumer-group")
            .WithWorkersCount(5)
            .WithBufferSize(100)
            .AddMiddlewares(middlewares => middlewares
            .AddSingleTypeDeserializer<OrderCreatedEvent, JsonCoreDeserializer>()
            .AddTypedHandlers(h => h
            .WithHandlerLifetime(InstanceLifetime.Scoped)
            .AddHandler<OrderCreatedConsumer>())
            )
        )

        .AddConsumer(consumer => consumer
            .Topic("order-updated-topic")
            .WithGroupId("inventory-consumer-group")
            .WithWorkersCount(5)
            .WithBufferSize(100)
            .AddMiddlewares(middlewares => middlewares
            .AddSingleTypeDeserializer<OrderUpdatedEvent, JsonCoreDeserializer>()
            .AddTypedHandlers(h => h
            .WithHandlerLifetime(InstanceLifetime.Scoped)
            .AddHandler<OrderUpdatedConsumer>())
            )
        )

    )
);

builder.Services.AddControllers();
// 1. Lấy cấu hình từ appsettings
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings");
var connectionString = mongoSettings["ConnectionString"];
var databaseName = mongoSettings["DatabaseName"];

// 2. Đăng ký IMongoClient (Singleton là chuẩn nhất cho Client)
builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));

// 3. Đăng ký IMongoDatabase (Lấy từ Client đã đăng ký ở trên)
builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(databaseName);
});

builder.Services.AddMediatR(cfg =>
{
    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    cfg.RegisterServicesFromAssemblies(assemblies);
});
var redisConfig = builder.Configuration.GetSection("Redis")["Configuration"] ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConfig)
);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfig;
});

builder.Services.AddScoped<IWareHouseRepository, WareHouseRepository>();

builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ICacheLockService, CacheLockService>();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.MapControllers();

app.Run();
