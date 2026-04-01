using System;

namespace WebAppExam.InventoryService.Application.Interfaces;

public interface ICacheLockService
{
    Task<List<string>> AcquireMultipleLocksAsync(IEnumerable<string> lockKeys, string lockToken, TimeSpan expiry);
    Task ReleaseMultipleLocksAsync(IEnumerable<string> lockKeys, string lockToken);
}
