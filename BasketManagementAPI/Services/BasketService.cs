using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Domain.Discounts;
using BasketManagementAPI.Repositories;
using BasketManagementAPI.Shipping;

namespace BasketManagementAPI.Services;

public sealed class BasketService : IBasketService
{
    private const decimal VatRate = 0.20m;

    private readonly IBasketRepository _repository;
    private readonly IShippingPolicy _shippingPolicy;
    private readonly IDiscountDefinitionRepository _discountDefinitionRepository;

    public BasketService(
        IBasketRepository repository,
        IShippingPolicy shippingPolicy,
        IDiscountDefinitionRepository discountDefinitionRepository)
    {
        _repository = repository;
        _shippingPolicy = shippingPolicy;
        _discountDefinitionRepository = discountDefinitionRepository;
    }

    public async Task<Basket> CreateBasketAsync()
    {
        var basket = new Basket();
        await _repository.CreateAsync(basket);
        return basket;
    }

    public async Task<Basket> AddItemsAsync(Guid basketId, IEnumerable<ItemDefinition> items)
    {
        if (items is null)
        {
            throw new ArgumentNullException(nameof(items));
        }

        var basket = await _repository.GetAsync(basketId);

        foreach (var itemDefinition in items)
        {
            var discount = CreateItemDiscount(itemDefinition.ItemDiscount);
            var item = new Item(
                itemDefinition.ProductId,
                itemDefinition.Name,
                itemDefinition.UnitPrice,
                itemDefinition.Quantity,
                discount);

            basket.AddOrUpdateItem(item);
        }

        await _repository.SaveAsync(basket);
        return basket;
    }

    public async Task<Basket> RemoveItemAsync(Guid basketId, string productId)
    {
        var basket = await _repository.GetAsync(basketId);

        if (!basket.RemoveItem(productId))
        {
            throw new KeyNotFoundException($"Item '{productId}' not found in basket.");
        }

        await _repository.SaveAsync(basket);
        return basket;
    }

    public async Task<Basket> ApplyDiscountCodeAsync(Guid basketId, string code, decimal percentage)
    {
        var basket = await _repository.GetAsync(basketId);
        var discount = new PercentageBasketDiscount(code, percentage);
        var discountDefinitionId = await _discountDefinitionRepository.UpsertAsync(code, percentage);
        basket.ApplyDiscount(discount, discountDefinitionId);
        await _repository.SaveAsync(basket);
        return basket;
    }

    public async Task<Totals> AddShippingAsync(Guid basketId, string country)
    {
        var basket = await _repository.GetAsync(basketId);
        var shipping = _shippingPolicy.Resolve(country);
        basket.SetShipping(shipping);
        await _repository.SaveAsync(basket);
        return await BuildTotalsAsync(basket);
    }

    public async Task<Totals> GetTotalsAsync(Guid basketId)
    {
        var basket = await _repository.GetAsync(basketId);
        return await BuildTotalsAsync(basket);
    }

    public async Task<Item> ApplyItemDiscountAsync(Guid basketId, string productId, ItemDiscountDefinition discount)
    {
        var basket = await _repository.GetAsync(basketId);
        var item = basket.Items.FirstOrDefault(i => string.Equals(i.ProductId, productId, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            throw new KeyNotFoundException($"Item '{productId}' was not found in basket '{basketId}'.");
        }

        var discountEngine = CreateItemDiscount(discount);

        if (discountEngine is null)
        {
            throw new InvalidOperationException("Unable to resolve item discount.");
        }

        item.ApplyDiscount(discountEngine);
        await _repository.SaveAsync(basket);
        return item;
    }

    public async Task<BasketSnapshot> GetBasketAsync(Guid basketId)
    {
        var basket = await _repository.GetAsync(basketId);
        var totals = await BuildTotalsAsync(basket);
        return new BasketSnapshot(basket, totals);
    }

    private static IBasketItemDiscount? CreateItemDiscount(ItemDiscountDefinition? definition)
    {
        if (definition is null)
        {
            return null;
        }

        return definition.Type switch
        {
            ItemDiscountType.FlatAmount => new FlatAmountItemDiscount(definition.Amount),
            ItemDiscountType.Bogo => new BuyOneGetOneFreeItemDiscount(),
            _ => throw new NotSupportedException($"Item discount type '{definition.Type}' is not supported.")
        };
    }

    private async Task<Totals> BuildTotalsAsync(Basket basket)
    {
        var subtotal = basket.Items.Sum(item => item.Total());
        var eligibleAmount = basket.Items.Where(item => !item.HasItemDiscount).Sum(item => item.Total());
        var discount = await CalculateBasketDiscountAsync(basket, eligibleAmount);
        var shipping = basket.ShippingDetails?.Cost ?? 0;
        var totalWithoutVat = Math.Max(subtotal - discount + shipping, 0);
        var vatBase = Math.Max(subtotal - discount, 0);
        var vatAmount = (int)Math.Round(vatBase * VatRate, 0, MidpointRounding.AwayFromZero);
        var totalWithVat = totalWithoutVat + vatAmount;

        return new Totals(subtotal, discount, shipping, totalWithoutVat, vatAmount, totalWithVat);
    }

    private async Task<int> CalculateBasketDiscountAsync(Basket basket, int eligibleAmount)
    {
        if (!basket.DiscountDefinitionId.HasValue)
        {
            return 0;
        }

        var definition = await _discountDefinitionRepository.GetByIdAsync(basket.DiscountDefinitionId.Value);
        if (definition is null || !definition.IsActive || definition.Percentage is null || definition.Percentage <= 0)
        {
            return 0;
        }

        var discountEngine = new PercentageBasketDiscount(definition.Code, definition.Percentage.Value);
        var calculated = discountEngine.CalculateDiscount(eligibleAmount);
        return Math.Min(calculated, eligibleAmount);
    }
}