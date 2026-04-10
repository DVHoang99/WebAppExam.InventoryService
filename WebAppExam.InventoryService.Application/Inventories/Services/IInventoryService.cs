using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using WebAppExam.InventoryService.Application.Inventories.DTOs;

namespace WebAppExam.InventoryService.Application.Interfaces
{
    public interface IInventoryService
    {
        Task<(bool IsSuccess, string FailReason)> CheckAndDeductInventoryAsync(IEnumerable<OrderItemDTO> items);
        Task<List<GetBatchInventoryDTO>> GetBatchInventoryDTOsByIdsAnsyc(List<string> ids, CancellationToken cancellationToken);
    }
}