using System;
using WebAppExam.InventoryService.Application.Orders.DTOs;

namespace WebAppExam.InventoryService.Application.Orders.Services;

public interface IOrderService
{
    Task SendMessageReply(OrderReplyDTO message, bool isSuccess, string reason);
}
