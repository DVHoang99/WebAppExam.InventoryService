using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Repositories;
using WebAppExam.InventoryService.Domain.Entity;
using WebAppExam.InventoryService.Infrastructure.Persistence;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class WareHouseRepository : BaseRepository<WareHouse>, IWareHouseRepository
{
    public WareHouseRepository(IMongoDatabase database, IMongoSessionProvider sessionProvider)
        : base(database, "WareHouses", sessionProvider)
    {
    }

    public async Task<List<WareHouse>> GetByIdsAync(List<string> ids, CancellationToken cancellationToken = default)
    {
        var filter = Builders<WareHouse>.Filter.In(x => x.Id, ids);
        var session = _sessionProvider.CurrentSession;
        if (session != null)
            return await _collection.Find(session, filter).ToListAsync(cancellationToken);
            
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }
}
