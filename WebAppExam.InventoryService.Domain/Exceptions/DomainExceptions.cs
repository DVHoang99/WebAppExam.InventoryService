using System;

namespace WebAppExam.InventoryService.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productId, int requested, int available) 
        : base($"Insufficient stock for product {productId}. Requested: {requested}, Available: {available}")
    {
    }
}
