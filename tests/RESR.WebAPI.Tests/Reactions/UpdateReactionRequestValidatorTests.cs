using RESR.Models.Reactions;
using RESR.WebAPI.Routes.Reactions.Validators;

namespace RESR.WebAPI.Tests.Reactions;

public sealed class UpdateReactionRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new UpdateReactionRequestValidator();
        var request = new UpdateReactionRequest(ReactionNames.Dislike);

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidRequest()
    {
        var validator = new UpdateReactionRequestValidator();
        var request = new UpdateReactionRequest("wow");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
