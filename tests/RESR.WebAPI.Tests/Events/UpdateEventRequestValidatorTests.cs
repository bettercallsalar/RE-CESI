using RESR.Models.Resources;
using RESR.WebAPI.Routes.Events.Validators;

namespace RESR.WebAPI.Tests.Events;

public sealed class UpdateEventRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_WhenAllOptionalFieldsNull()
    {
        var validator = new UpdateEventRequestValidator();
        var request = new UpdateEventRequest();

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidFields()
    {
        var validator = new UpdateEventRequestValidator();
        var request = new UpdateEventRequest(
            Title: null,
            Description: null,
            Visibility: "hidden",
            IdCategory: 0,
            Subtitle: null,
            StartDate: new DateTime(2026, 4, 10),
            EndDate: new DateTime(2026, 4, 9),
            Address: null,
            IdDepartment: 0);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 4);
    }
}
