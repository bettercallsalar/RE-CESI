using FluentValidation;
using RESR.Models.Resources;

namespace RESR.WebAPI.Routes.Articles.Validators;

public sealed class CreateArticleRequestValidator : AbstractValidator<CreateArticleRequest>
{
    public CreateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Visibility)
            .NotEmpty()
            .Must(BeValidVisibility)
            .WithMessage("Visibility must be PUBLIC or PRIVATE.");

        RuleFor(x => x.IdCategory)
            .GreaterThan(0);

        RuleFor(x => x.Content)
            .NotEmpty();
    }

    private static bool BeValidVisibility(string visibility) =>
        string.Equals(visibility, nameof(ResourceVisibility.PUBLIC), StringComparison.OrdinalIgnoreCase) ||
        string.Equals(visibility, nameof(ResourceVisibility.PRIVATE), StringComparison.OrdinalIgnoreCase);
}
