using BasketManagementAPI.Contracts.Requests;
using BasketManagementAPI.Domain.Discounts;
using FluentValidation;

namespace BasketManagementAPI.Validators;

public sealed class ItemDiscountRequestValidator : AbstractValidator<ItemDiscountRequest>
{
    public ItemDiscountRequestValidator()
    {
        RuleFor(discount => discount.Type)
            .IsInEnum()
            .WithMessage("Discount type must be one of: FlatAmount, Bogo.");

        RuleFor(discount => discount.Amount)
            .GreaterThan(0)
            .WithMessage("Discount amount must be greater than zero.");
    }
}