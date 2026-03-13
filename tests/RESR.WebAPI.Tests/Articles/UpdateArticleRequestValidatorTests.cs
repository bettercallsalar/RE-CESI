using RESR.Models.Resources;
using RESR.WebAPI.Routes.Articles.Validators;

namespace RESR.WebAPI.Tests.Articles;

public sealed class UpdateArticleRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_WhenAllOptionalFieldsNull()
    {
        var validator = new UpdateArticleRequestValidator();
        var request = new UpdateArticleRequest();

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidFields()
    {
        var validator = new UpdateArticleRequestValidator();
        var request = new UpdateArticleRequest(
            Title: new string('a', 60),
            Description: null,
            Visibility: "hidden",
            IdCategory: 0,
            Content: null);

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 3);
    }
}
