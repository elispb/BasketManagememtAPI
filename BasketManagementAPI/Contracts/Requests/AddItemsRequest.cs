using System.Collections.Generic;

namespace BasketManagementAPI.Contracts.Requests;

public sealed record AddItemsRequest(IEnumerable<AddItemRequest> Items);

public sealed record AddItemRequest(
    string Name,
    int UnitPrice,
    int Quantity,
    ItemDiscountRequest? ItemDiscount);


