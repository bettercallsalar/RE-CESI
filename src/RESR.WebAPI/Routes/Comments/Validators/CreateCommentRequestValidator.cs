using FluentValidation;
using RESR.Models.Comments;

namespace RESR.WebAPI.Routes.Comments.Validators;

public sealed class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.IdParentComment)
            .GreaterThan(0)
            .When(x => x.IdParentComment.HasValue);
    }
}
