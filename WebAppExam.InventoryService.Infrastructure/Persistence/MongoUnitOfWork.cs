using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace WebAppExam.InventoryService.Infrastructure.Persistence;

public class MongoUnitOfWork : IUnitOfWork
{
    private readonly IMongoSessionProvider _sessionProvider;
    private readonly ILogger<MongoUnitOfWork> _logger;
    private bool _disposed;

    public MongoUnitOfWork(IMongoSessionProvider sessionProvider, ILogger<MongoUnitOfWork> logger)
    {
        _sessionProvider = sessionProvider;
        _logger = logger;
    }

    public async Task StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        var session = await _sessionProvider.BeginSessionAsync(cancellationToken);
        
        // MongoDB transactions are only supported on Replica Sets or Sharded Clusters.
        // Standalone instances (common in local dev) will throw NotSupportedException.
        var clusterType = session.Client.Cluster.Description.Type;
        if (clusterType == MongoDB.Driver.Core.Clusters.ClusterType.ReplicaSet || 
            clusterType == MongoDB.Driver.Core.Clusters.ClusterType.Sharded)
        {
            session.StartTransaction();
        }
        else
        {
            _logger.LogWarning("MongoDB Transactions are skipped because the server is 'Standalone'. For atomicity, use a Replica Set.");
        }
    }

    public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
    {
        var session = _sessionProvider.CurrentSession;
        if (session != null && session.IsInTransaction)
        {
            await session.CommitTransactionAsync(cancellationToken);
            return true;
        }
        return false;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        var session = _sessionProvider.CurrentSession;
        if (session != null && session.IsInTransaction)
        {
            await session.AbortTransactionAsync(cancellationToken);
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _sessionProvider.CurrentSession?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
