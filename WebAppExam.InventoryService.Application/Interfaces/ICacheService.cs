using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebAppExam.InventoryService.Application.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T data, TimeSpan expiration, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string key, Func<Task<T>> functionToObtain, TimeSpan duration, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix);
    Task RemoveAsync(string key);
}
