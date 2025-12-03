using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;

namespace BasketManagementAPI.Services;

public interface ITotalsCalculator
{
    Task<Totals> CalculateAsync(Basket basket);
}

public sealed class TotalsCalculator : ITotalsCalculator
{
    private const decimal VatRate = 0.20m;

    private readonly IDiscountDefinitionService _discountDefinitionService;

    public TotalsCalculator(IDiscountDefinitionService discountDefinitionService)
    {
        _discountDefinitionService = discountDefinitionService
            ?? throw new ArgumentNullException(nameof(discountDefinitionService));
    }

    public async Task<Totals> CalculateAsync(Basket basket)
    {
        ArgumentNullException.ThrowIfNull(basket);

        var subtotal = basket.Items.Sum(item => item.Total());
        var eligibleAmount = basket.Items.Where(item => !item.HasItemDiscount).Sum(item => item.Total());
        var discount = await _discountDefinitionService.CalculateDiscountAsync(basket.DiscountDefinitionId, eligibleAmount);
        var shipping = basket.ShippingDetails?.Cost ?? 0;
        var totalWithoutVat = Math.Max(subtotal - discount + shipping, 0);
        var vatBase = Math.Max(subtotal - discount, 0);
        var vatAmount = (int)Math.Round(vatBase * VatRate, 0, MidpointRounding.AwayFromZero);
        var totalWithVat = totalWithoutVat + vatAmount;

        return new Totals(subtotal, discount, shipping, totalWithoutVat, vatAmount, totalWithVat);
    }
}

