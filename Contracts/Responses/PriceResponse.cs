namespace BasketManagementAPI.Contracts.Responses;

public sealed record PriceResponse(
    decimal Subtotal,
    decimal Discount,
    decimal Shipping,
    decimal TotalWithoutVat,
    decimal VatAmount,
    decimal TotalWithVat);

