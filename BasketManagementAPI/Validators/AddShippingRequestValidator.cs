using BasketManagementAPI.Contracts.Requests;
using FluentValidation;

namespace BasketManagementAPI.Validators;

public sealed class AddShippingRequestValidator : AbstractValidator<AddShippingRequest>
{
    public AddShippingRequestValidator()
    {
        RuleFor(request => request.CountryCode)
            .NotEmpty()
            .WithMessage("Country code is required for shipping.");
    }
}
