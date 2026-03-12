using RESR.Models.Resources;
using RESR.WebAPI.Routes.Articles.Validators;

namespace RESR.WebAPI.Tests.Articles;

public sealed class CreateArticleRequestValidatorTests
{
    [Fact]
    public void Validate_Passes_ForValidRequest()
    {
        var validator = new CreateArticleRequestValidator();
        var request = new CreateArticleRequest(
            "Article title",
            "Description",
            "PUBLIC",
            2,
            "Content");

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_Fails_ForInvalidRequest()
    {
        var validator = new CreateArticleRequestValidator();
        var request = new CreateArticleRequest(
            "",
            null,
            "hidden",
            0,
            "");

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 4);
    }
}
