using BasketManagementAPI.Domain.Discounts;

namespace BasketManagementAPI.Services;

public sealed record ItemDefinition(
    string ProductId,
    string Name,
    int UnitPrice,
    int Quantity,
    ItemDiscountDefinition? ItemDiscount);

public sealed record ItemDiscountDefinition(ItemDiscountType Type, int Amount);


