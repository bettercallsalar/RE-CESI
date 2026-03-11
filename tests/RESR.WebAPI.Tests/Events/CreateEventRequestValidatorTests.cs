using RESR.Models.Resources;
using RESR.WebAPI.Routes.Events.Validators;

namespace RESR.WebAPI.Tests.Events;

public sealed class CreateEventRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new CreateEventRequestValidator();
        var request = new CreateEventRequest(
            "Event title",
            "Description",
            "PRIVATE",
            1,
            2,
            "Subtitle",
            new DateTime(2026, 3, 1),
            new DateTime(2026, 3, 2),
            "Paris",
            75);

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidRequest()
    {
        var validator = new CreateEventRequestValidator();
        var request = new CreateEventRequest(
            "",
            null,
            "hidden",
            0,
            0,
            null,
            new DateTime(2026, 3, 2),
            new DateTime(2026, 3, 1),
            null,
            0);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 5);
    }
}
