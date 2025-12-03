using System.Linq;
using BasketManagementAPI.Contracts.Requests;
using BasketManagementAPI.Contracts.Responses;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasketManagementAPI.Controllers;

/// <summary>
/// Manages baskets, including creation, shipping, and discounts.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class BasketsController : ControllerBase
{
    private readonly IBasketService _basketService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasketsController"/> class.
    /// </summary>
    /// <param name="basketService">Provides basket management operations.</param>
    public BasketsController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    /// <summary>
    /// Creates an empty basket and returns a reference to it.
    /// </summary>
    /// <returns>A <see cref="CreatedAtActionResult"/> with the new basket identifier.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBasket()
    {
        var basket = await _basketService.CreateBasketAsync();
        return CreatedAtAction(
            nameof(GetBasket),
            new { basketId = basket.Id },
            new { basketId = basket.Id });
    }

    /// <summary>
    /// Applies a discount code to an existing basket.
    /// </summary>
    /// <param name="basketId">Identifier of the basket to update.</param>
    /// <param name="request">
    ///     Request payload containing the discount code to apply and the percentage that should be removed from the basket
    ///     total (greater than 0 and no more than 100).
    /// </param>
    /// <returns>A <see cref="NoContentResult"/> when the discount is applied.</returns>
    [HttpPatch("{basketId:guid}/discount")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ApplyDiscount(Guid basketId, ApplyDiscountRequest request)
    {
        await _basketService.ApplyDiscountCodeAsync(basketId, request.Code, request.Percentage);
        return NoContent();
    }

    /// <summary>
    /// Adds shipping costs to the specified basket.
    /// </summary>
    /// <param name="basketId">Identifier of the basket to update.</param>
    /// <param name="request">
    ///     Request payload containing the destination country code. The provided code is required, trimmed, normalized,
    ///     and used to look up the shipping rate before returning updated totals. Both enum names (`UnitedKingdom`, `uk`)
    ///     and integer values (`1`) from the `CountryCode` enum are accepted.
    /// </param>
    /// <returns>The updated basket totals after shipping is applied.</returns>
    [HttpPatch("{basketId:guid}/shipping")]
    [ProducesResponseType(typeof(PriceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddShipping(Guid basketId, AddShippingRequest request)
    {
        var totals = await _basketService.AddShippingAsync(basketId, request.CountryCode);
        return Ok(MapPrice(totals));
    }

    /// <summary>
    /// Retrieves the current state of a basket.
    /// </summary>
    /// <param name="basketId">Identifier of the basket to return.</param>
    /// <returns>The basket snapshot including items, shipping and discounts.</returns>
    [HttpGet("{basketId:guid}")]
    [ProducesResponseType(typeof(BasketResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBasket(Guid basketId)
    {
        var snapshot = await _basketService.GetBasketAsync(basketId);
        return Ok(Map(snapshot));
    }

    /// <summary>
    /// Retrieves all current baskets.
    /// </summary>
    /// <returns>A list of basket snapshots.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BasketResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBaskets()
    {
        var snapshots = await _basketService.GetAllBasketsAsync();
        var response = snapshots.Select(Map).ToList();
        return Ok(response);
    }

    private static BasketResponse Map(BasketSnapshot basketSnapshot)
    {
        return new BasketResponse(
            basketSnapshot.Basket.Id,
            basketSnapshot.Basket.Items.Select(Map).ToList(),
            basketSnapshot.Basket.ShippingDetails is null
                ? null
                : new ShippingDetailsResponse(
                    basketSnapshot.Basket.ShippingDetails.CountryCode.ToString(),
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


