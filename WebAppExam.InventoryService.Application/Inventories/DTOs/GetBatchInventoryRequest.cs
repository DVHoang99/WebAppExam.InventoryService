using System;

namespace WebAppExam.InventoryService.Application.Inventories.DTOs;

public class GetBatchInventoryRequest
{
    public List<string> CorrelationIds { get; set; }
}
