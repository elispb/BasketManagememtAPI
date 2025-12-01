using System;

namespace BasketManagementAPI.Contracts.Responses;

public sealed record BasketResponse(
    Guid Id,
    IEnumerable<BasketItemResponse> Items,
    ShippingDetailsResponse? Shipping,
    string? DiscountCode,
    PriceResponse Totals);

public sealed record BasketItemResponse(
    string ProductId,
    string Name,
    decimal UnitPrice,
    int Quantity,
    decimal Total,
    bool HasDiscount,
    string? DiscountDescription);

public sealed record ShippingDetailsResponse(string Country, decimal Cost);

