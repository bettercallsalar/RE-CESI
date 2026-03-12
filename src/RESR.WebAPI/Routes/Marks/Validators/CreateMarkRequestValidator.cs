using FluentValidation;
using RESR.Models.Marks;

namespace RESR.WebAPI.Routes.Marks.Validators;

public sealed class CreateMarkRequestValidator : AbstractValidator<CreateMarkRequest>
{
    public CreateMarkRequestValidator()
    {
        RuleFor(x => x.IdRessource)
            .GreaterThan(0);

        RuleFor(x => x)
            .Must(x => x.IsFavorite || x.IsReadLater)
            .WithMessage("At least one mark must be set.");
    }
}
