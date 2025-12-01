using BasketManagementAPI.Contracts.Requests;
using FluentValidation;

namespace BasketManagementAPI.Validators;

public sealed class ItemDiscountRequestValidator : AbstractValidator<ItemDiscountRequest>
{
    private static readonly string[] SupportedTypes = { "flatamount", "bogo" };

    public ItemDiscountRequestValidator()
    {
        RuleFor(discount => discount.Type)
            .NotEmpty()
            .WithMessage("Discount type is required.")
            .Must(BeSupportedType)
            .WithMessage("Discount type must be one of: FlatAmount, Bogo.");

        RuleFor(discount => discount.Amount)
            .GreaterThan(0)
            .WithMessage("Discount amount must be greater than zero.");
    }

    private static bool BeSupportedType(string? type)
    {
        if (type is null)
        {
            return false;
        }

        return SupportedTypes.Contains(type.Trim(), StringComparer.OrdinalIgnoreCase);
    }
}

