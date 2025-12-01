namespace BasketManagementAPI.Services;

public sealed record BasketItemDefinition(
    string ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    ItemDiscountDefinition? ItemDiscount);

public sealed record ItemDiscountDefinition(string Type, decimal Amount);

