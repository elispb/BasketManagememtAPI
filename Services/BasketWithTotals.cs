using BasketManagementAPI.Domain.Baskets;

namespace BasketManagementAPI.Services;

public sealed record BasketWithTotals(Basket Basket, BasketTotals Totals);

