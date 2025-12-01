namespace BasketManagementAPI.Domain.Discounts;

public interface IBasketDiscount
{
    string Code { get; }

    decimal CalculateDiscount(decimal eligibleAmount);
}

