using System;
using StackExchange.Redis;
using WebAppExam.InventoryService.Application.Interfaces;

namespace WebAppExam.InventoryService.Infrastructure.Services;

public class CacheLockService : ICacheLockService
{
    private readonly IDatabase _redisDb;
    private const int MaxRetry = 3;
    private const int RetryDelayMs = 200;
    public CacheLockService(IConnectionMultiplexer redis)
    {
        _redisDb = redis.GetDatabase();
    }

    public async Task<List<string>> AcquireMultipleLocksAsync(IEnumerable<string> lockKeys, string lockToken, TimeSpan expiry)
    {
        var acquiredKeys = new List<string>();

        foreach (var key in lockKeys)
        {
            bool isLocked = false;

            for (int i = 0; i < MaxRetry; i++)
            {
                if (await _redisDb.LockTakeAsync(key, lockToken, expiry))
                {
                    isLocked = true;
                    break;
                }
                await Task.Delay(RetryDelayMs);
            }

            if (isLocked)
            {
                acquiredKeys.Add(key);
            }
            else
            {
                await ReleaseMultipleLocksAsync(acquiredKeys, lockToken);
                return new List<string>();
            }
        }

        return acquiredKeys;
    }

    public async Task ReleaseMultipleLocksAsync(IEnumerable<string> lockKeys, string lockToken)
    {
        var tasks = lockKeys.Select(key => _redisDb.LockReleaseAsync(key, lockToken));
        await Task.WhenAll(tasks);
    }
}
