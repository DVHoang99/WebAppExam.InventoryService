namespace WebAppExam.InventoryService.Infrastructure.Constants;

public static class DatabaseSettings
{
    // Redis
    public const string DefaultRedisConnection = "localhost:6379";

    // MongoDB
    public const string DefaultMongoConnection = "mongodb://admin:adminpassword@localhost:27017";
    public const string DefaultDatabaseName = "InventoryDB";
}
