using RESR.Models.Marks;
using RESR.WebAPI.Routes.Marks.Validators;

namespace RESR.WebAPI.Tests.Marks;

public sealed class UpdateMarkRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new UpdateMarkRequestValidator();
        var request = new UpdateMarkRequest(true, false, 4);

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenNoMarksSelected()
    {
        var validator = new UpdateMarkRequestValidator();
        var request = new UpdateMarkRequest(false, false, 4);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenResourceInvalid()
    {
        var validator = new UpdateMarkRequestValidator();
        var request = new UpdateMarkRequest(true, false, 0);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
