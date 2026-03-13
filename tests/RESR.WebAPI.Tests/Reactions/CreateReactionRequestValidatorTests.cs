using RESR.Models.Reactions;
using RESR.WebAPI.Routes.Reactions.Validators;

namespace RESR.WebAPI.Tests.Reactions;

public sealed class CreateReactionRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new CreateReactionRequestValidator();
        var request = new CreateReactionRequest(ReactionNames.Love);

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidRequest()
    {
        var validator = new CreateReactionRequestValidator();
        var request = new CreateReactionRequest("angry");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
