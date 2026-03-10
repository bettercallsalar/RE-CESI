using FluentValidation;
using RESR.Models.Users;

namespace RESR.WebAPI.Routes.Users.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches(@"^[a-zA-Z0-9_]+$")
            .When(x => x.Username is not null);

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(255)
            .When(x => x.Email is not null);

        RuleFor(x => x.FirstName)
            .MinimumLength(2)
            .MaximumLength(100)
            .Matches(@"^[a-zA-ZÀ-ÿ'\\ -]+$")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio is not null);

        RuleFor(x => x.IdDepartment)
            .GreaterThan(0)
            .When(x => x.IdDepartment.HasValue);

        RuleFor(x => x.IdRole)
            .GreaterThan(0)
            .When(x => x.IdRole.HasValue);
    }
}
