using BasketManagementAPI.Domain.Baskets;

namespace BasketManagementAPI.Services;

public sealed record BasketSnapshot(Basket Basket, Totals Totals);


