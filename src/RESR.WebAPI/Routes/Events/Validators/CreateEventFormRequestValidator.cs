using FluentValidation;

namespace RESR.WebAPI.Routes.Events.Validators;

public sealed class CreateEventFormRequestValidator : AbstractValidator<CreateEventFormRequest>
{
    public CreateEventFormRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description is not null);
        RuleFor(x => x.Visibility)
            .NotEmpty()
            .Must(value => string.Equals(value, "PUBLIC", StringComparison.OrdinalIgnoreCase) || string.Equals(value, "PRIVATE", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Visibility must be PUBLIC or PRIVATE.");
        RuleFor(x => x.IdCategory).GreaterThan(0);
        RuleFor(x => x.Subtitle).MaximumLength(255).When(x => x.Subtitle is not null);
        RuleFor(x => x.Address).MaximumLength(255).When(x => x.Address is not null);
        RuleFor(x => x.IdDepartment).GreaterThan(0).When(x => x.IdDepartment.HasValue);
        RuleFor(x => x.Images).Must(images => images is null || images.Count <= 6).WithMessage("Vous ne pouvez pas envoyer plus de 6 images.");
        RuleFor(x => x)
            .Must(x => x.EndDate is null || x.EndDate >= x.StartDate)
            .WithMessage("EndDate cannot be earlier than StartDate.");
    }
}
