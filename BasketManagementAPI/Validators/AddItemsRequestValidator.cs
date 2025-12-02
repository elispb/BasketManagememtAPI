using BasketManagementAPI.Contracts.Requests;
using FluentValidation;

namespace BasketManagementAPI.Validators;

public sealed class AddItemsRequestValidator : AbstractValidator<AddItemsRequest>
{
    public AddItemsRequestValidator()
    {
        RuleFor(request => request.Items)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one item must be supplied.");

        RuleForEach(request => request.Items)
            .SetValidator(new AddItemRequestValidator());
    }
}
