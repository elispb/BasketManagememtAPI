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

    private static BasketResponse Map(BasketWithTotals basketWithTotals)
    {
        return new BasketResponse(
            basketWithTotals.Basket.Id,
            basketWithTotals.Basket.Items.Select(Map).ToList(),
            basketWithTotals.Basket.ShippingDetails is null
                ? null
                : new ShippingDetailsResponse(
                    basketWithTotals.Basket.ShippingDetails.Country,
                    basketWithTotals.Basket.ShippingDetails.Cost),
            basketWithTotals.Basket.BasketDiscount?.Code,
            MapPrice(basketWithTotals.Totals));
    }

    private static BasketItemResponse Map(BasketItem item)
    {
        return new BasketItemResponse(
            item.ProductId,
            item.Name,
            item.UnitPrice,
            item.Quantity,
            item.Total(),
            item.HasItemDiscount,
            item.ItemDiscount?.Description);
    }

    private static PriceResponse MapPrice(BasketTotals totals)
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

