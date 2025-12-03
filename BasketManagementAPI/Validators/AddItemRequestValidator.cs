using BasketManagementAPI.Contracts.Requests;
using FluentValidation;

namespace BasketManagementAPI.Validators;

public sealed class AddItemRequestValidator : AbstractValidator<AddItemRequest>
{
    public AddItemRequestValidator()
    {
        RuleFor(item => item.Name)
            .NotEmpty()
            .WithMessage("Product name is required.");

        RuleFor(item => item.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Unit price must be greater than zero.");

        RuleFor(item => item.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than zero.");

        When(item => item.ItemDiscount is not null, () =>
        {
            RuleFor(item => item.ItemDiscount!)
                .SetValidator(new ItemDiscountRequestValidator());
        });
    }
}
