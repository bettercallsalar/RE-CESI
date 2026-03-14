using FluentValidation;
using RESR.Models.Resources;

namespace RESR.WebAPI.Routes.Events.Validators;

public sealed class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
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

        RuleFor(x => x.Subtitle)
            .MaximumLength(255)
            .When(x => x.Subtitle is not null);

        RuleFor(x => x.Address)
            .MaximumLength(255)
            .When(x => x.Address is not null);

        RuleFor(x => x.IdDepartment)
            .GreaterThan(0)
            .When(x => x.IdDepartment.HasValue);

        RuleFor(x => x)
            .Must(x => x.StartDate is null || x.EndDate is null || x.EndDate > x.StartDate)
            .WithMessage("EndDate must be later than StartDate.");
    }

    private static bool BeValidVisibility(string? visibility) =>
        visibility is not null &&
        (string.Equals(visibility, nameof(ResourceVisibility.PUBLIC), StringComparison.OrdinalIgnoreCase) ||
         string.Equals(visibility, nameof(ResourceVisibility.PRIVATE), StringComparison.OrdinalIgnoreCase));
}
