
using WebAppExam.InventoryService.Domain.Common;

namespace WebAppExam.InventoryService.Application.Interfaces;

public interface IWareHouseRepository : IBaseRepository<Domain.Entity.WareHouse>
{
    Task<List<Domain.Entity.WareHouse>> GetByIdsAync(List<string> ids, CancellationToken cancellationToken = default);
}
