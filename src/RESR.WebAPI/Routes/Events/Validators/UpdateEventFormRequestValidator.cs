using FluentValidation;

namespace RESR.WebAPI.Routes.Events.Validators;

public sealed class UpdateEventFormRequestValidator : AbstractValidator<UpdateEventFormRequest>
{
    public UpdateEventFormRequestValidator()
    {
        RuleFor(x => x.Title).MaximumLength(50).When(x => x.Title is not null);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description is not null);
        RuleFor(x => x.Visibility)
            .Must(value => value is not null && (string.Equals(value, "PUBLIC", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "PRIVATE", StringComparison.OrdinalIgnoreCase)))
            .WithMessage("Visibility must be PUBLIC or PRIVATE.")
            .When(x => x.Visibility is not null);
        RuleFor(x => x.IdCategory).GreaterThan(0).When(x => x.IdCategory.HasValue);
        RuleFor(x => x.Subtitle).MaximumLength(255).When(x => x.Subtitle is not null);
        RuleFor(x => x.Address).MaximumLength(255).When(x => x.Address is not null);
        RuleFor(x => x.IdDepartment).GreaterThan(0).When(x => x.IdDepartment.HasValue);
        RuleFor(x => x.Images).Must(images => images is null || images.Count <= 6).WithMessage("Vous ne pouvez pas envoyer plus de 6 images.");
        RuleFor(x => x.DefaultImageId)
            .GreaterThan(0)
            .When(x => x.DefaultImageId.HasValue)
            .WithMessage("L'image par defaut selectionnee est invalide.");
        RuleFor(x => x.DefaultImageIndex)
            .GreaterThanOrEqualTo(0)
            .When(x => x.DefaultImageIndex.HasValue)
            .WithMessage("L'image par defaut selectionnee est invalide.");
        RuleFor(x => x)
            .Must(x => x.StartDate is null || x.EndDate is null || x.EndDate > x.StartDate)
            .WithMessage("EndDate must be later than StartDate.");
    }
}
