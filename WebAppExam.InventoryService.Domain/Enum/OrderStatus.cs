namespace WebAppExam.InventoryService.Domain.Enum;

public enum OrderStatus
{
    Draft = 0,
    Cancel = 1,
    Pending = 2,
    WaitingForPayment = 3,
    Paid = 4,
    Failed = 5
}
