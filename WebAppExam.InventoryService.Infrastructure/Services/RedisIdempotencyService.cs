using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Infrastructure.Constants;

namespace WebAppExam.InventoryService.Infrastructure.Services;

public class RedisIdempotencyService : IIdempotencyService
{
    private readonly IDatabase _redisDb;

    public RedisIdempotencyService(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public async Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken = default)
    {
        var redisKey = $"{CacheKeys.IdempotencyPrefix}{key}";
        return await _redisDb.KeyExistsAsync(redisKey);
    }

    public async Task MarkAsProcessedAsync(string key, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var redisKey = $"{CacheKeys.IdempotencyPrefix}{key}";
        var ttl = expiry ?? TimeSpan.FromDays(CommonConstants.IdempotencyTtlDays);
        await _redisDb.StringSetAsync(redisKey, "1", ttl);
    }
}
