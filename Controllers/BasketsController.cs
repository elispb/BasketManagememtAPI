using System.Linq;
using BasketManagementAPI.Contracts.Requests;
using BasketManagementAPI.Contracts.Responses;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BasketManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class BasketsController : ControllerBase
{
    private readonly IBasketService _basketService;

    public BasketsController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBasket()
    {
        var basket = await _basketService.CreateBasketAsync();
        return CreatedAtAction(
            nameof(GetBasket),
            new { basketId = basket.Id },
            new { basketId = basket.Id });
    }

    [HttpPatch("{basketId:guid}/discount")]
    public async Task<IActionResult> ApplyDiscount(Guid basketId, ApplyDiscountRequest request)
    {
        await _basketService.ApplyDiscountCodeAsync(basketId, request.Code, request.Percentage);
        return NoContent();
    }

    [HttpPatch("{basketId:guid}/shipping")]
    public async Task<IActionResult> AddShipping(Guid basketId, AddShippingRequest request)
    {
        var totals = await _basketService.AddShippingAsync(basketId, request.Country);
        return Ok(MapPrice(totals));
    }

    [HttpGet("{basketId:guid}")]
    public async Task<IActionResult> GetBasket(Guid basketId)
    {
        var snapshot = await _basketService.GetBasketAsync(basketId);
        return Ok(Map(snapshot));
    }

    private static BasketResponse Map(BasketSnapshot basketSnapshot)
    {
        return new BasketResponse(
            basketSnapshot.Basket.Id,
            basketSnapshot.Basket.Items.Select(Map).ToList(),
            basketSnapshot.Basket.ShippingDetails is null
                ? null
                : new ShippingDetailsResponse(
                    basketSnapshot.Basket.ShippingDetails.Country,
                    basketSnapshot.Basket.ShippingDetails.Cost),
            basketSnapshot.Basket.BasketDiscount?.Code,
            MapPrice(basketSnapshot.Totals));
    }

    private static ItemResponse Map(Item item)
    {
        return new ItemResponse(
            item.ProductId,
            item.Name,
            item.UnitPrice,
            item.Quantity,
            item.Total(),
            item.HasItemDiscount,
            item.ItemDiscount?.Description);
    }

    private static PriceResponse MapPrice(Totals totals)
    {
        return new PriceResponse(
            totals.Subtotal,
            totals.Discount,
            totals.Shipping,
            totals.TotalWithoutVat,
            totals.VatAmount,
            totals.TotalWithVat);
    }
}

