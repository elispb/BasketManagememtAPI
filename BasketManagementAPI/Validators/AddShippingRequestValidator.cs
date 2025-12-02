using BasketManagementAPI.Contracts.Requests;
using FluentValidation;

namespace BasketManagementAPI.Validators;

public sealed class AddShippingRequestValidator : AbstractValidator<AddShippingRequest>
{
    public AddShippingRequestValidator()
    {
        RuleFor(request => request.Country)
            .NotEmpty()
            .WithMessage("Country is required for shipping.");
    }
}

