using FluentValidation;

namespace RESR.WebAPI.Routes.Users.Validators;

internal static class UserValidationRules
{
    private const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

    public static IRuleBuilderOptions<T, string?> ApplyOptionalEmailRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(255)
            .WithMessage("L'adresse e-mail ne peut pas depasser 255 caracteres.")
            .Matches(EmailPattern)
            .WithMessage("Le format de l'adresse e-mail est invalide.");
    }

    public static IRuleBuilderOptions<T, string> ApplyRequiredEmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("L'adresse e-mail est obligatoire.")
            .MaximumLength(255)
            .WithMessage("L'adresse e-mail ne peut pas depasser 255 caracteres.")
            .Matches(EmailPattern)
            .WithMessage("Le format de l'adresse e-mail est invalide.");
    }
}
