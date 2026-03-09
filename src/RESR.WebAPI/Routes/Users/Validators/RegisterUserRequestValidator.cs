using FluentValidation;
using RESR.Models.Users;

namespace RESR.WebAPI.Routes.Users.Validators;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(30)
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username can contain only letters, numbers, and underscore.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
            .Matches(@"^[a-zA-ZÀ-ÿ'\\ -]+$")
            .WithMessage("FirstName contains invalid characters.");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .When(x => x.Bio is not null);

        RuleFor(x => x.IdDepartment)
            .GreaterThan(0);

        RuleFor(x => x.IdRole)
            .GreaterThan(0);
    }
}
