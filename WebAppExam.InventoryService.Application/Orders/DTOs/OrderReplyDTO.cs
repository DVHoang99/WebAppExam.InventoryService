using System;
using System.Diagnostics.CodeAnalysis;
using WebAppExam.InventoryService.Domain.Enum;

namespace WebAppExam.InventoryService.Application.Orders.DTOs;

public class OrderReplyDTO
{
    public required string OrderId { get; init; }
    public OrderStatus Status { get; init; }
    public required string Reason { get; init; }
    public required string Action { get; init; }
    public string IdenpotencyId { get; init; }
    public required OrderDetailDTO Data { get; init; }

    public OrderReplyDTO() { }

    [SetsRequiredMembers]
    private OrderReplyDTO(string orderId, OrderStatus status, string reason, string action, string idenpotencyId, OrderDetailDTO data)
    {
        OrderId = orderId;
        Status = status;
        Reason = reason;
        Action = action;
        IdenpotencyId = idenpotencyId;
        Data = data;
    }


    public static OrderReplyDTO FromResult(string orderId, OrderStatus status, string reason, string action, OrderDetailDTO data)
    {
        var idenpotencyId = Ulid.NewUlid().ToString();
        return new OrderReplyDTO(orderId, status, reason, action, idenpotencyId, data);
    }
}
