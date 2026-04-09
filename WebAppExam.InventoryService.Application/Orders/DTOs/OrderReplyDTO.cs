using System;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Application.Orders.DTOs;

public class OrderReplyDTO
{
    public string OrderId { get; init; }
    public OrderStatus Status { get; init; }
    public string Reason { get; init; }
    public string Action { get; init; }
    public List<OrderDetailDTO> Data { get; init; }

    private OrderReplyDTO(string orderId, OrderStatus status, string reason, string action, List<OrderDetailDTO> data)
    {
        OrderId = orderId;
        Status = status;
        Reason = reason;
        Action = action;
        Data = data;
    }

    public static OrderReplyDTO FromResult(string orderId, OrderStatus status, string reason, string action, List<OrderDetailDTO> data)
    {
        return new OrderReplyDTO(orderId, status, reason, action, data);
    }
}
