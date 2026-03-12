using RESR.Models.Comments;
using RESR.WebAPI.Routes.Comments.Validators;

namespace RESR.WebAPI.Tests.Comments;

public sealed class UpdateCommentRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new UpdateCommentRequestValidator();
        var request = new UpdateCommentRequest("Updated");

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidRequest()
    {
        var validator = new UpdateCommentRequestValidator();
        var request = new UpdateCommentRequest(string.Empty);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
