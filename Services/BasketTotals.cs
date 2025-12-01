namespace BasketManagementAPI.Services;

public sealed record BasketTotals(
    int Subtotal,
    int Discount,
    int Shipping,
    int TotalWithoutVat,
    int VatAmount,
    int TotalWithVat);

