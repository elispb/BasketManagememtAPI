namespace BasketManagementAPI.Domain.Discounts;

public sealed record BuyOneGetOneFreeItemDiscount : IBasketItemDiscount
{
    public string Description => "Buy one get one free";

    public decimal CalculateTotal(decimal unitPrice, int quantity)
    {
        var payableQuantity = (quantity / 2) + (quantity % 2);
        return payableQuantity * unitPrice;
    }
}

