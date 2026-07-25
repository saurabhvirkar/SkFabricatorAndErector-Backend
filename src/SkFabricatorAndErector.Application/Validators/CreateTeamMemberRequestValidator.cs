using FluentValidation;
using SkFabricatorAndErector.Application.Contracts.Requests.Catalog;

namespace SkFabricatorAndErector.Application.Validators;

public class CreateTeamMemberRequestValidator : AbstractValidator<CreateTeamMemberRequest>
{
    public CreateTeamMemberRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Team member name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .MaximumLength(100).WithMessage("Role cannot exceed 100 characters.");
    }
}
