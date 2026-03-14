using FluentValidation;

namespace RESR.WebAPI.Routes.Articles.Validators;

public sealed class CreateArticleFormRequestValidator : AbstractValidator<CreateArticleFormRequest>
{
    public CreateArticleFormRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description is not null);
        RuleFor(x => x.Visibility)
            .NotEmpty()
            .Must(value => string.Equals(value, "PUBLIC", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "PRIVATE", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Visibility must be PUBLIC or PRIVATE.");
        RuleFor(x => x.IdCategory).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty();
        RuleFor(x => x.Images).Must(images => images is null || images.Count <= 6).WithMessage("Vous ne pouvez pas envoyer plus de 6 images.");
        RuleFor(x => x.DefaultImageIndex)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DefaultImageIndex.HasValue)
            .WithMessage("L'image par defaut selectionnee est invalide.");
    }
}
