namespace BasketManagememtAPI.Contracts.Responses;

public sealed record ItemPriceResponse(
    int LineTotal,
    int VatAmount,
    int TotalWithVat);

