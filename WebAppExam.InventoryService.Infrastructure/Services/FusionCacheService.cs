using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;
using ZiggyCreatures.Caching.Fusion;
using Microsoft.Extensions.Logging;

namespace WebAppExam.InventoryService.Infrastructure.Services;

public class FusionCacheService : ICacheService
{
    private readonly IFusionCache _fusionCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<FusionCacheService> _logger;

    public FusionCacheService(IFusionCache fusionCache, IConnectionMultiplexer redis, ILogger<FusionCacheService> logger)
    {
        _fusionCache = fusionCache;
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        return await _fusionCache.GetOrDefaultAsync<T>(key, default, token: cancellationToken);
    }

    public async Task SetAsync<T>(string key, T data, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        await _fusionCache.SetAsync(key, data, expiration, token: cancellationToken);
    }

    public async Task<T?> GetAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        return await _fusionCache.GetOrSetAsync<T>(
            key,
            async ct => await functionToObtain(),
            duration,
            token: cancellationToken);
    }

    public async Task RemoveAsync(string key)
    {
        await _fusionCache.RemoveAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints.First());
        var db = _redis.GetDatabase();
        var keys = server.Keys(database: db.Database, pattern: $"{prefix}*").ToArray();

        if (keys.Any())
        {
            await db.KeyDeleteAsync(keys);
            
            // Also remove from local L1 cache on all instances via FusionCache
            foreach (var key in keys)
            {
                await _fusionCache.RemoveAsync(key.ToString());
            }
        }
    }
}
