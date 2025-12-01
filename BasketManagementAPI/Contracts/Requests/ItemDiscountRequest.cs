using BasketManagementAPI.Domain.Discounts;

namespace BasketManagementAPI.Contracts.Requests;

public sealed record ItemDiscountRequest(ItemDiscountType Type, int Amount);
