namespace BasketManagementAPI.Contracts.Requests;

public sealed record ApplyDiscountRequest(string Code, decimal Percentage);