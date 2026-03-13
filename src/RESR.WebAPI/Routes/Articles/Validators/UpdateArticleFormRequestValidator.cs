using FluentValidation;

namespace RESR.WebAPI.Routes.Articles.Validators;

public sealed class UpdateArticleFormRequestValidator : AbstractValidator<UpdateArticleFormRequest>
{
    public UpdateArticleFormRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(50).When(x => x.Title is not null);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description is not null);
        RuleFor(x => x.Visibility)
            .Must(value => value is not null && (string.Equals(value, "PUBLIC", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "PRIVATE", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Visibility must be PUBLIC or PRIVATE.")
            .When(x => x.Visibility is not null);
        RuleFor(x => x.IdCategory).GreaterThan(0).When(x => x.IdCategory.HasValue);
        RuleFor(x => x.Images).Must(images => images is null || images.Count <= 6).WithMessage("Vous ne pouvez pas envoyer plus de 6 images.");
    }
}
