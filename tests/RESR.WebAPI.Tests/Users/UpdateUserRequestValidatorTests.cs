using RESR.Models.Users;
using RESR.WebAPI.Routes.Users.Validators;

namespace RESR.WebAPI.Tests.Users;

public sealed class UpdateUserRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_WhenAllOptionalFieldsNull()
    {
        var validator = new UpdateUserRequestValidator();
        var request = new UpdateUserRequest(null, null, null, null, null, null, null);

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidProvidedFields()
    {
        var validator = new UpdateUserRequestValidator();
        var request = new UpdateUserRequest(
            "u!",
            ".",
            "1",
            null,
            new string('a', 600),
            0,
            0
        );

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 5);
    }
}
