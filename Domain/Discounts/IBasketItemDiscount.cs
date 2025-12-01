namespace BasketManagementAPI.Domain.Discounts;

public interface IBasketItemDiscount
{
    string Description { get; }

    int CalculateTotal(int unitPrice, int quantity);
}


