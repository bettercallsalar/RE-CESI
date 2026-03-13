using FluentValidation;
using RESR.Models.Users;

namespace RESR.WebAPI.Routes.Users.Validators;

public sealed class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
{
    public RegisterUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Le nom d'utilisateur est obligatoire.")
            .MinimumLength(3)
            .WithMessage("Le nom d'utilisateur doit contenir au moins 3 caracteres.")
            .MaximumLength(30)
            .WithMessage("Le nom d'utilisateur ne peut pas depasser 30 caracteres.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Le nom d'utilisateur peut contenir uniquement des lettres, des chiffres et des underscores.");

        RuleFor(x => x.Email)
            .ApplyRequiredEmailRules();

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Le mot de passe est obligatoire.")
            .MinimumLength(8)
            .WithMessage("Le mot de passe doit contenir au moins 8 caracteres.")
            .MaximumLength(100);

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Le prenom est obligatoire.")
            .MinimumLength(2)
            .WithMessage("Le prenom doit contenir au moins 2 caracteres.")
            .MaximumLength(100)
            .WithMessage("Le prenom ne peut pas depasser 100 caracteres.")
            .Matches(@"^[a-zA-ZÀ-ÿ'\\ -]+$")
            .WithMessage("Le prenom contient des caracteres invalides.");

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("La biographie ne peut pas depasser 500 caracteres.")
            .When(x => x.Bio is not null);

        RuleFor(x => x.IdDepartment)
            .GreaterThan(0)
            .WithMessage("Le departement doit etre superieur a 0.");
    }
}
