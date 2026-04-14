namespace WebAppExam.InventoryService.Application.Interfaces;

public interface IIdempotencyService
{
    /// <summary>
    /// Checks if the message has already been processed.
    /// </summary>
    Task<bool> IsProcessedAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks the message as processed with a TTL.
    /// </summary>
    Task MarkAsProcessedAsync(string key, TimeSpan? expiry = null, CancellationToken cancellationToken = default);
}
