using RESR.Models.Users;
using RESR.WebAPI.Routes.Users.Validators;

namespace RESR.WebAPI.Tests.Users;

public sealed class RegisterUserRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new RegisterUserRequestValidator();
        var request = new RegisterUserRequest(
            "user_name",
            "user@example.com",
            "password1",
            "Jean",
            new DateOnly(2000, 1, 1),
            "bio",
            1
        );

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidFields()
    {
        var validator = new RegisterUserRequestValidator();
        var request = new RegisterUserRequest(
            "u!",
            ".",
            "short",
            "1",
            null,
            new string('a', 501),
            0
        );

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 6);
    }
}
