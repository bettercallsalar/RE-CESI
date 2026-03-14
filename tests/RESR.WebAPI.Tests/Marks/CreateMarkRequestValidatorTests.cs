using RESR.Models.Marks;
using RESR.WebAPI.Routes.Marks.Validators;

namespace RESR.WebAPI.Tests.Marks;

public sealed class CreateMarkRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new CreateMarkRequestValidator();
        var request = new CreateMarkRequest(true, false, 4);

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenNoMarksSelected()
    {
        var validator = new CreateMarkRequestValidator();
        var request = new CreateMarkRequest(false, false, 4);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_WhenResourceInvalid()
    {
        var validator = new CreateMarkRequestValidator();
        var request = new CreateMarkRequest(true, false, 0);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
