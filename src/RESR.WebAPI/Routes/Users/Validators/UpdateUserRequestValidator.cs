using FluentValidation;
using RESR.Models.Users;

namespace RESR.WebAPI.Routes.Users.Validators;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        RuleFor(x => x.Username)
            .MinimumLength(3)
            .WithMessage("Le nom d'utilisateur doit contenir au moins 3 caracteres.")
            .MaximumLength(30)
            .WithMessage("Le nom d'utilisateur ne peut pas depasser 30 caracteres.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Le nom d'utilisateur peut contenir uniquement des lettres, des chiffres et des underscores.")
            .When(x => x.Username is not null);

        RuleFor(x => x.Email)
            .ApplyOptionalEmailRules()
            .When(x => x.Email is not null);

        RuleFor(x => x.FirstName)
            .MinimumLength(2)
            .WithMessage("Le prenom doit contenir au moins 2 caracteres.")
            .MaximumLength(100)
            .WithMessage("Le prenom ne peut pas depasser 100 caracteres.")
            .Matches(@"^[a-zA-ZÀ-ÿ'\\ -]+$")
            .WithMessage("Le prenom contient des caracteres invalides.")
            .When(x => x.FirstName is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage("La biographie ne peut pas depasser 500 caracteres.")
            .When(x => x.Bio is not null);

        RuleFor(x => x.IdDepartment)
            .GreaterThan(0)
            .WithMessage("Le departement doit etre superieur a 0.")
            .When(x => x.IdDepartment.HasValue);

        RuleFor(x => x.IdRole)
            .GreaterThan(0)
            .WithMessage("Le role doit etre superieur a 0.")
            .When(x => x.IdRole.HasValue);
    }
}
