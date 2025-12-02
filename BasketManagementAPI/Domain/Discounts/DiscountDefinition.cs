using System;

namespace BasketManagementAPI.Domain.Discounts;

public sealed record DiscountDefinition(
    Guid Id,
    string Code,
    decimal? Percentage,
    string? Metadata,
    bool IsActive);

