using System;
using System.Text.Json.Serialization;

namespace WebAppExam.InventoryService.Application.Orders.DTOs;

public class OrderDetailDTO
{
    public string ProductId { get; init; }

    public int Quantity { get; init; }

    public int Price { get; init; }
    public string WareHouseId { get; init; }

    public decimal SubTotal => Quantity * Price;

    private OrderDetailDTO(string productId, int quantity, int price, string wareHouseId)
    {
        ProductId = productId;
        Quantity = quantity;
        Price = price;
        WareHouseId = wareHouseId;
    }

    public static OrderDetailDTO FromResult(string productId, int quantity, int price, string wareHouseId)
    {
        return new OrderDetailDTO(productId, quantity, price, wareHouseId);
    }
}
