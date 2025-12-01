namespace BasketManagementAPI.Contracts.Responses;

public sealed record PriceResponse(
    int Subtotal,
    int Discount,
    int Shipping,
    int TotalWithoutVat,
    int VatAmount,
    int TotalWithVat);


