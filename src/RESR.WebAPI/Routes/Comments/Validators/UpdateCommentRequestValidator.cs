using FluentValidation;
using RESR.Models.Comments;

namespace RESR.WebAPI.Routes.Comments.Validators;

public sealed class UpdateCommentRequestValidator : AbstractValidator<UpdateCommentRequest>
{
    public UpdateCommentRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(2000);
    }
}
