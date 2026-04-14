namespace WebAppExam.InventoryService.Infrastructure.Constants;

public static class CommonConstants
{
    public const int LockTimeoutSeconds = 30;
    public const string InternalKeyHeader = "X-Internal-Key";
    public const string InternalApiKeyConfigPath = "InternalSettings:ApiKey";
    public const int IdempotencyTtlDays = 1;
}
