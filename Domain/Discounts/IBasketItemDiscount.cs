namespace BasketManagementAPI.Domain.Discounts;

public interface IBasketItemDiscount
{
    string Description { get; }

    decimal CalculateTotal(decimal unitPrice, int quantity);
}

