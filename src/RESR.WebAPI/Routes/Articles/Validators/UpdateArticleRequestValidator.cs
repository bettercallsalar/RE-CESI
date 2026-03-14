using FluentValidation;
using RESR.Models.Resources;

namespace RESR.WebAPI.Routes.Articles.Validators;

public sealed class UpdateArticleRequestValidator : AbstractValidator<UpdateArticleRequest>
{
    public UpdateArticleRequestValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(50)
            .When(x => x.Title is not null);

        RuleFor(x => x.Description)
            .MaximumLength(5000)
            .When(x => x.Description is not null);

        RuleFor(x => x.Visibility)
            .Must(BeValidVisibility)
            .WithMessage("Visibility must be PUBLIC or PRIVATE.")
            .When(x => x.Visibility is not null);

        RuleFor(x => x.IdCategory)
            .GreaterThan(0)
            .When(x => x.IdCategory.HasValue);
    }

    private static bool BeValidVisibility(string? visibility) =>
        visibility is not null &&
        (string.Equals(visibility, nameof(ResourceVisibility.PUBLIC), StringComparison.OrdinalIgnoreCase) ||
         string.Equals(visibility, nameof(ResourceVisibility.PRIVATE), StringComparison.OrdinalIgnoreCase));
}
