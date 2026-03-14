using FluentValidation;
using RESR.Models.Reactions;

namespace RESR.WebAPI.Routes.Reactions.Validators;

public sealed class CreateReactionRequestValidator : AbstractValidator<CreateReactionRequest>
{
    public CreateReactionRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(name => ReactionNames.All.Contains(name.Trim().ToLowerInvariant()))
            .WithMessage("Name must be one of: like, dislike, love.");
    }
}
