namespace WebAppExam.InventoryService.Infrastructure.Constants;

public static class CacheKeys
{
    public const string IdempotencyLockPrefix = "lock:idempotency:";
    public const string IdempotencyPrefix = "idempotency:";
}
