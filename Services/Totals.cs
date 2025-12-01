namespace BasketManagementAPI.Services;

public sealed record Totals(
    int Subtotal,
    int Discount,
    int Shipping,
    int TotalWithoutVat,
    int VatAmount,
    int TotalWithVat);


