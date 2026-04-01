using System;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Interfaces;
using WebAppExam.InventoryService.Domain.Entity;

namespace WebAppExam.InventoryService.Infrastructure.Repositories;

public class WareHouseRepository : BaseRepository<WareHouse>, IWareHouseRepository
{
    public WareHouseRepository(IMongoDatabase database)
        : base(database, "WareHouses")
    {
    }
    public async Task<List<WareHouse>> GetByIdsAync(List<string> ids, CancellationToken cancellationToken = default)
    {
        var filter = Builders<WareHouse>.Filter.In(x => x.Id, ids);
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }
}
