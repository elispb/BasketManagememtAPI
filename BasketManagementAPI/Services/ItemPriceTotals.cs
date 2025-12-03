namespace BasketManagementAPI.Services;

public sealed record ItemPriceTotals(
    int LineTotal,
    int VatAmount,
    int TotalWithVat);

