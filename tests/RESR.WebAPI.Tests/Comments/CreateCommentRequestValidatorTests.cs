using RESR.Models.Comments;
using RESR.WebAPI.Routes.Comments.Validators;

namespace RESR.WebAPI.Tests.Comments;

public sealed class CreateCommentRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest("Hello", 1);

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Passes_WhenParentCommentIsOmitted()
    {
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest("Hello");

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Null(request.IdParentComment);
    }

    [Fact]
    public void Validate_Fails_ForInvalidRequest()
    {
        var validator = new CreateCommentRequestValidator();
        var request = new CreateCommentRequest(string.Empty, 0);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }
}
