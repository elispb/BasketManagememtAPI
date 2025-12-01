using System.Collections.Generic;

namespace BasketManagememtAPI.Contracts.Requests;

public sealed record AddItemsRequest(IEnumerable<AddItemRequest> Items);

public sealed record AddItemRequest(
    string ProductId,
    string Name,
    int UnitPrice,
    int Quantity,
    ItemDiscountRequest? ItemDiscount);

