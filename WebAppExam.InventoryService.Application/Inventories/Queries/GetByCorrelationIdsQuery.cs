using System;
using MediatR;
using WebAppExam.InventoryService.Application.Inventories.DTOs;

namespace WebAppExam.InventoryService.Application.Inventories.Queries;

public class GetByCorrelationIdsQuery(GetBatchInventoryRequest request) : IRequest<List<GetBatchInventoryDTO>>
{
    public List<string> CorrelationIds { get; set; } = request.CorrelationIds;
}
