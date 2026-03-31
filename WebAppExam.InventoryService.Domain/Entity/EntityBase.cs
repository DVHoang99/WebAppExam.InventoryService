using System;

namespace WebAppExam.InventoryService.Domain.Entity;

public class EntityBase
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
