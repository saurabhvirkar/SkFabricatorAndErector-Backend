using FluentValidation;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;

namespace SkFabricatorAndErector.Application.Validators;

public class CreateOurServiceRequestValidator : AbstractValidator<CreateOurServiceRequest>
{
    public CreateOurServiceRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(100).WithMessage("Service name cannot exceed 100 characters.");
    }
}
