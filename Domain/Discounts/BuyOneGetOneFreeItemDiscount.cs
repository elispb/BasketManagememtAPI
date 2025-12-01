namespace BasketManagememtAPI.Domain.Discounts;

public sealed record BuyOneGetOneFreeItemDiscount : IBasketItemDiscount
{
    public string Description => "Buy one get one free";

    public int CalculateTotal(int unitPrice, int quantity)
    {
        var payableQuantity = (quantity / 2) + (quantity % 2);
        return payableQuantity * unitPrice;
    }
}

