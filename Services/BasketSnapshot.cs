using BasketManagememtAPI.Domain.Baskets;

namespace BasketManagememtAPI.Services;

public sealed record BasketSnapshot(Basket Basket, Totals Totals);

