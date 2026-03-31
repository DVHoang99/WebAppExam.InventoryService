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
}
