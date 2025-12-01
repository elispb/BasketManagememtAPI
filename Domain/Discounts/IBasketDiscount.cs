namespace BasketManagementAPI.Domain.Discounts;

public interface IBasketDiscount
{
    string Code { get; }

    int CalculateDiscount(int eligibleAmount);
}

