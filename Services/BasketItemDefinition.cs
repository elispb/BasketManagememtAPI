namespace BasketManagementAPI.Services;

public sealed record BasketItemDefinition(
    string ProductId,
    string Name,
    int UnitPrice,
    int Quantity,
    ItemDiscountDefinition? ItemDiscount);

public sealed record ItemDiscountDefinition(string Type, int Amount);

