using System;
using System.Linq;
using BasketManagementAPI.Contracts.Requests;
using BasketManagementAPI.Contracts.Responses;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BasketManagementAPI.Controllers;

/// <summary>
/// Handles item-level operations for a specific basket.
/// </summary>
[ApiController]
[Route("api/baskets/{basketId:guid}/items")]
public sealed class ItemsController : ControllerBase
{
    private readonly IBasketService _basketService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ItemsController"/> class.
    /// </summary>
    /// <param name="basketService">Provides basket services used by item operations.</param>
    public ItemsController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    /// <summary>
    /// Adds or updates items inside the specified basket.
    /// </summary>
    /// <param name="basketId">Identifier of the basket to modify.</param>
    /// <param name="request">Details of the items to add.</param>
    /// <returns><see cref="OkObjectResult"/> containing the created items when the request succeeds.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItems(Guid basketId, AddItemsRequest request)
    {
        if (request.Items is null || !request.Items.Any())
        {
            return BadRequest("At least one item must be supplied.");
        }

        var addedItems = await _basketService.AddItemsAsync(basketId, request.Items.Select(Map));
        var response = addedItems.Select(Map).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Retrieves a specific item from a basket.
    /// </summary>
    /// <param name="basketId">Identifier of the basket.</param>
    /// <param name="productId">Identifier of the product to return.</param>
    /// <returns>The wrapped item if it exists; otherwise <see cref="NotFoundResult"/>.</returns>
    [HttpGet("{productId:int}")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItem(Guid basketId, int productId)
    {
        var item = await _basketService.GetItemAsync(basketId, productId);
        if (item is null)
        {
            return NotFound();
        }

        return Ok(Map(item));
    }

    /// <summary>
    /// Returns pricing details for an item in the basket.
    /// </summary>
    /// <param name="basketId">Identifier of the basket.</param>
    /// <param name="productId">Identifier of the product to price.</param>
    /// <returns><see cref="ItemPriceResponse"/> if the item exists; otherwise <see cref="NotFoundResult"/>.</returns>
    [HttpGet("{productId:int}/price")]
    [ProducesResponseType(typeof(ItemPriceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItemPrice(Guid basketId, int productId)
    {
        var price = await _basketService.GetItemTotalsAsync(basketId, productId);
        if (price is null)
        {
            return NotFound();
        }

        return Ok(new ItemPriceResponse(price.LineTotal, price.VatAmount, price.TotalWithVat));
    }

    /// <summary>
    /// Applies an item-level discount definition.
    /// </summary>
    /// <param name="basketId">Identifier of the basket.</param>
    /// <param name="productId">Identifier of the item to discount.</param>
    /// <param name="request">Discount definition payload.</param>
    /// <returns>The updated item wrapped inside an <see cref="ItemResponse"/>.</returns>
    [HttpPatch("{productId:int}/discount")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ApplyItemDiscount(Guid basketId, int productId, ItemDiscountRequest request)
    {
        var item = await _basketService.ApplyItemDiscountAsync(
            basketId,
            productId,
            new ItemDiscountDefinition(request.Type, request.Amount));

        return Ok(Map(item));
    }

    /// <summary>
    /// Removes an item from a basket.
    /// </summary>
    /// <param name="basketId">Identifier of the basket.</param>
    /// <param name="productId">Identifier of the product to remove.</param>
    /// <returns><see cref="NoContentResult"/> once the item is deleted.</returns>
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveItem(Guid basketId, int productId)
    {
        await _basketService.RemoveItemAsync(basketId, productId);
        return NoContent();
    }

    private static ItemDefinition Map(AddItemRequest request)
    {
        return new ItemDefinition(
            request.Name,
            request.UnitPrice,
            request.Quantity,
            request.ItemDiscount is null
                ? null
                : new ItemDiscountDefinition(request.ItemDiscount.Type, request.ItemDiscount.Amount));
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

}


