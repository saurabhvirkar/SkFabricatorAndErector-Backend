using FluentValidation;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;

namespace SkFabricatorAndErector.Application.Validators;

public class CreateClientDetailsRequestValidator : AbstractValidator<CreateClientDetailsRequest>
{
    public CreateClientDetailsRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Client name is required.")
            .MaximumLength(100).WithMessage("Client name cannot exceed 100 characters.");
    }
}
