using FluentValidation;

namespace RESR.WebAPI.Routes.Users.Validators;

internal static class UserValidationRules
{
    private const string EmailPattern = @"^[^\s@]+@[^\s@]+\.[^\s@]+$";

    public static IRuleBuilderOptions<T, string?> ApplyOptionalEmailRules<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(255)
            .Matches(EmailPattern)
            .WithMessage("Email format is invalid.");
    }

    public static IRuleBuilderOptions<T, string> ApplyRequiredEmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .MaximumLength(255)
            .Matches(EmailPattern)
            .WithMessage("Email format is invalid.");
    }
}
