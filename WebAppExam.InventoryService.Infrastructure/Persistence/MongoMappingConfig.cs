using System;
using MongoDB.Bson.Serialization;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Persistence;

public static class MongoMappingConfig
{
    public static void Configure()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Inventory)))
        {
            BsonClassMap.RegisterClassMap<Inventory>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(c => c.Id);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(WareHouse)))
        {
            BsonClassMap.RegisterClassMap<WareHouse>(cm =>
            {
                cm.AutoMap();
                cm.MapIdProperty(c => c.Id);
            });
        }
    }
}
