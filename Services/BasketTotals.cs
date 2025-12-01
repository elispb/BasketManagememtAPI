namespace BasketManagementAPI.Services;

public sealed record BasketTotals(
    decimal Subtotal,
    decimal Discount,
    decimal Shipping,
    decimal TotalWithoutVat,
    decimal VatAmount,
    decimal TotalWithVat);

