namespace BasketManagementAPI.Contracts.Responses;

public sealed record ItemPriceResponse(
    decimal LineTotal,
    decimal VatAmount,
    decimal TotalWithVat);

