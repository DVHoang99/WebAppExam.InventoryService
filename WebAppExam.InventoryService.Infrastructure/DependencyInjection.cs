using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Orders.Services;
using WebAppExam.InventoryService.Infrastructure.Constants;
using WebAppExam.InventoryService.Infrastructure.Repositories;
using WebAppExam.InventoryService.Infrastructure.Services;
using InventoryService = WebAppExam.InventoryService.Infrastructure.Services.InventoryService;

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

        // Register Repositories
        services.AddScoped<IWareHouseRepository, WareHouseRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();

        // Register Services
        services.AddScoped<ICacheLockService, CacheLockService>();
        services.AddScoped<IInventoryService, Services.InventoryService>();
        services.AddScoped<IOrderService, OrderService>();

        return services;
    }
}
