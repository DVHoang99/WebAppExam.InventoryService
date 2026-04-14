using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Infrastructure.Constants;
using WebAppExam.InventoryService.Infrastructure.Repositories;
using WebAppExam.InventoryService.Infrastructure.Services;
using Hangfire;
using Hangfire.Redis.StackExchange;
using WebAppExam.InventoryService.Infrastructure.Persistence;

namespace WebAppExam.InventoryService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure MongoDB
        var mongoSettings = configuration.GetSection("MongoDbSettings");
        var connectionString = mongoSettings["ConnectionString"];
        var databaseName = mongoSettings["DatabaseName"];

        services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        // Configure Redis
        var redisConfig = configuration.GetSection("Redis")["Configuration"] ?? DatabaseSettings.DefaultRedisConnection;

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConfig)
        );

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConfig;
        });

        // Configure Hangfire storage (using Redis)
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseRedisStorage(redisConfig, new RedisStorageOptions
            {
                Prefix = "hangfire:inventory:",
                Db = 0 // Using default DB for Hangfire, or you can specify another
            })
        );

        // Register Repositories
        services.AddScoped<IWareHouseRepository, WareHouseRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddScoped<IMongoSessionProvider, MongoSessionProvider>();
        services.AddScoped<IUnitOfWork, MongoUnitOfWork>();

        // Register Services
        services.AddScoped<ICacheLockService, CacheLockService>();
        services.AddScoped<IIdempotencyService, RedisIdempotencyService>();
        services.AddScoped<IInventoryService, Services.InventoryService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
