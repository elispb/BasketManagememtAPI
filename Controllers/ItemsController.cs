using System;
using System.Linq;
using BasketManagementAPI.Contracts.Requests;
using BasketManagementAPI.Contracts.Responses;
using BasketManagementAPI.Domain.Baskets;
using BasketManagementAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BasketManagementAPI.Controllers;

[ApiController]
[Route("api/baskets/{basketId:guid}/items")]
public sealed class ItemsController : ControllerBase
{
    private readonly IBasketService _basketService;

    public ItemsController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    [HttpPost]
    public async Task<IActionResult> AddItems(Guid basketId, AddItemsRequest request)
    {
        if (request.Items is null || !request.Items.Any())
        {
            return BadRequest("At least one item must be supplied.");
        }

        await _basketService.AddItemsAsync(basketId, request.Items.Select(Map));
        return NoContent();
    }

    [HttpGet("{productId}")]
    public async Task<IActionResult> GetItem(Guid basketId, string productId)
    {
        var snapshot = await _basketService.GetBasketAsync(basketId);
        var item = snapshot.Basket.Items
            .FirstOrDefault(i => string.Equals(i.ProductId, productId, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            return NotFound();
        }

        return Ok(Map(item));
    }

    [HttpGet("{productId}/price")]
    public async Task<IActionResult> GetItemPrice(Guid basketId, string productId)
    {
        var snapshot = await _basketService.GetBasketAsync(basketId);
        var item = snapshot.Basket.Items
            .FirstOrDefault(i => string.Equals(i.ProductId, productId, StringComparison.OrdinalIgnoreCase));

        if (item is null)
        {
            return NotFound();
        }

        return Ok(MapPrice(item));
    }

    [HttpPatch("{productId}/discount")]
    public async Task<IActionResult> ApplyItemDiscount(Guid basketId, string productId, ItemDiscountRequest request)
    {
        var item = await _basketService.ApplyItemDiscountAsync(
            basketId,
            productId,
            new ItemDiscountDefinition(request.Type, request.Amount));

        return Ok(Map(item));
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> RemoveItem(Guid basketId, string productId)
    {
        await _basketService.RemoveItemAsync(basketId, productId);
        return NoContent();
    }

    private static ItemDefinition Map(AddItemRequest request)
    {
        return new ItemDefinition(
            request.ProductId,
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

    private static ItemPriceResponse MapPrice(Item item)
    {
        var lineTotal = item.Total();
        var vatAmount = (int)Math.Round(lineTotal * 0.20m, 0, MidpointRounding.AwayFromZero);
        return new ItemPriceResponse(lineTotal, vatAmount, lineTotal + vatAmount);
    }
}


