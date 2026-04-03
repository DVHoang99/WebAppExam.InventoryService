
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Infrastructure.Repositories;
using KafkaFlow;
using KafkaFlow.Serializer;
using WebAppExam.InventoryService.Infrastructure.Consumers;
using WebAppExam.InventoryService.Infrastructure.Services;
using StackExchange.Redis;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderUpdatedConsumer;
using WebAppExam.InventoryService.Infrastructure.Consumers.OrderDeletedComsumer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);
var kafkaBrokers = builder.Configuration.GetSection("KafkaConfig:Brokers").Get<string[]>()
                   ?? new[] { "localhost:9092" };

builder.Services.AddKafka(kafka => kafka
    .UseConsoleLog()
    .AddCluster(cluster => cluster
        .WithBrokers(kafkaBrokers)
        .CreateTopicIfNotExists("order-reply-topic", 3, 1)
        .CreateTopicIfNotExists("order-updated-reply-topic", 3, 1)
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
            .WithGroupId("inventory-order-created-group")
            .WithWorkersCount(2)
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
            .WithGroupId("inventory-order-updated-group")
            .WithWorkersCount(2)
            .WithBufferSize(100)
            .AddMiddlewares(middlewares => middlewares
            .AddSingleTypeDeserializer<OrderUpdatedEvent, JsonCoreDeserializer>()
            .AddTypedHandlers(h => h
            .WithHandlerLifetime(InstanceLifetime.Scoped)
            .AddHandler<OrderUpdatedConsumer>())
            )
        )

        .AddConsumer(consumer => consumer
            .Topic("order-deleted-topic")
            .WithGroupId("inventory-order-deleted-group")
            .WithWorkersCount(2)
            .WithBufferSize(100)
            .AddMiddlewares(middlewares => middlewares
            .AddSingleTypeDeserializer<OrderDeletedEvent, JsonCoreDeserializer>()
            .AddTypedHandlers(h => h
            .WithHandlerLifetime(InstanceLifetime.Scoped)
            .AddHandler<OrderDeletedConsumer>())
            )
        )

    )
);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization(options =>
{
    var sharedPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAssertion(context =>
        {
            var httpContext = context.Resource as HttpContext;
            if (httpContext == null) return false;

            if (httpContext.User.Identity?.IsAuthenticated == true) return true;

            if (httpContext.Request.Headers.TryGetValue("X-Internal-Key", out var extractedKey))
            {
                var secretKey = builder.Configuration["InternalSettings:ApiKey"];
                return !string.IsNullOrEmpty(secretKey) && extractedKey == secretKey;
            }

            return false;
        })
        .Build();

    options.FallbackPolicy = sharedPolicy;
});


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

app.UseAuthentication();
app.UseAuthorization();
var kafkaBus = app.Services.CreateKafkaBus();
await kafkaBus.StartAsync();

app.MapControllers();

app.Run();
