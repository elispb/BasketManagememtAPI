using BasketManagementAPI.Contracts.Requests;
using FluentValidation;

namespace BasketManagementAPI.Validators;

public sealed class ApplyDiscountRequestValidator : AbstractValidator<ApplyDiscountRequest>
{
    public ApplyDiscountRequestValidator()
    {
        RuleFor(request => request.Code)
            .NotEmpty()
            .WithMessage("Discount code is required.");

        RuleFor(request => request.Percentage)
            .GreaterThan(0)
            .WithMessage("Percentage must be greater than zero.")
            .LessThanOrEqualTo(100)
            .WithMessage("Percentage cannot exceed 100.");
    }
}
