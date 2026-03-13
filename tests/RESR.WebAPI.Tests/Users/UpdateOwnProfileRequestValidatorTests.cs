using RESR.Models.Users;
using RESR.WebAPI.Routes.Users.Validators;

namespace RESR.WebAPI.Tests.Users;

public sealed class UpdateOwnProfileRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_WhenAllOptionalFieldsNull()
    {
        var validator = new UpdateOwnProfileRequestValidator();
        var request = new UpdateOwnProfileRequest(null, null, null, null, null, null);

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidProvidedFields()
    {
        var validator = new UpdateOwnProfileRequestValidator();
        var request = new UpdateOwnProfileRequest(
            "u!",
            ".",
            "1",
            null,
            new string('a', 600),
            0
        );

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 5);
    }
}
