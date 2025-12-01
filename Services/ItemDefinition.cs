namespace BasketManagememtAPI.Services;

public sealed record ItemDefinition(
    string ProductId,
    string Name,
    int UnitPrice,
    int Quantity,
    ItemDiscountDefinition? ItemDiscount);

public sealed record ItemDiscountDefinition(string Type, int Amount);

