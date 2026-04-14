using MongoDB.Driver;

namespace WebAppExam.InventoryService.Infrastructure.Persistence;

public interface IMongoSessionProvider
{
    IClientSessionHandle? CurrentSession { get; }
    Task<IClientSessionHandle> BeginSessionAsync(CancellationToken cancellationToken = default);
}

public class MongoSessionProvider : IMongoSessionProvider
{
    private readonly IMongoClient _client;
    public IClientSessionHandle? CurrentSession { get; private set; }

    public MongoSessionProvider(IMongoClient client)
    {
        _client = client;
    }

    public async Task<IClientSessionHandle> BeginSessionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentSession == null)
        {
            CurrentSession = await _client.StartSessionAsync(cancellationToken: cancellationToken);
        }
        return CurrentSession;
    }
}
