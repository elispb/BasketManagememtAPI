using System;

namespace BasketManagementAPI.Contracts.Responses;

public sealed record BasketResponse(
    Guid Id,
    IEnumerable<ItemResponse> Items,
    ShippingDetailsResponse? Shipping,
    string? DiscountCode,
    PriceResponse Totals);

public sealed record ItemResponse(
    string ProductId,
    string Name,
    int UnitPrice,
    int Quantity,
    int Total,
    bool HasDiscount,
    string? DiscountDescription);

public sealed record ShippingDetailsResponse(string Country, int Cost);


