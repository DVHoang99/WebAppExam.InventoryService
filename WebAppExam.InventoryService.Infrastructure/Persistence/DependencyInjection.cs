using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Infrastructure.Repositories;

namespace WebAppExam.InventoryService.Infrastructure.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Apply MongoDB mappings
        MongoMappingConfig.Configure();

        // 2. Register MongoDbContext as Singleton
        var connectionString = configuration["MongoDbSettings:ConnectionString"] ?? throw new InvalidOperationException("MongoDB connection string is not configured.");
        var databaseName = configuration["MongoDbSettings:DatabaseName"] ?? throw new InvalidOperationException("MongoDB database name is not configured.");

        services.AddSingleton(new MongoDbContext(connectionString, databaseName));

        // 3. Register Repositories
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IWareHouseRepository, WareHouseRepository>();

        return services;
    }
}
