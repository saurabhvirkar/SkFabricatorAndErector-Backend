using FluentValidation;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;

namespace SkFabricatorAndErector.Application.Validators;

public class CreateProjectRequestValidator : AbstractValidator<CreateProjectRequest>
{
    public CreateProjectRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Project title is required.")
            .MaximumLength(150).WithMessage("Project title cannot exceed 150 characters.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Project category is required.");
    }
}
