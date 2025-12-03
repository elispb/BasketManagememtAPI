using System;
using BasketManagementAPI.Domain.Baskets;

namespace BasketManagementAPI.Domain.Discounts;

public static class ItemDiscountFactory
{
    public static IBasketItemDiscount? Create(byte? type, int? amount)
    {
        if (!type.HasValue)
        {
            return null;
        }

        return Create((ItemDiscountType)type.Value, amount);
    }

    public static IBasketItemDiscount Create(ItemDiscountType type, int? amount)
    {
        return type switch
        {
            ItemDiscountType.FlatAmount => new FlatAmountItemDiscount(amount ?? 0),
            ItemDiscountType.Bogo => new BuyOneGetOneFreeItemDiscount(),
            _ => throw new NotSupportedException($"Item discount type '{type}' is not supported.")
        };
    }

    public static (byte? Type, int? Amount) ToPersistedData(IBasketItemDiscount? discount)
    {
        return discount switch
        {
            FlatAmountItemDiscount flat => ((byte)ItemDiscountType.FlatAmount, flat.AmountTaken),
            BuyOneGetOneFreeItemDiscount => ((byte)ItemDiscountType.Bogo, 0),
            null => (null, null),
            _ => throw new NotSupportedException("Unsupported item discount type.")
        };
    }
}

