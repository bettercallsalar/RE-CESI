using Microsoft.AspNetCore.Http;

namespace RESR.WebAPI.Routes.Articles;

public sealed class CreateArticleFormRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Visibility { get; init; } = "PUBLIC";
    public int IdCategory { get; init; }
    public string Content { get; init; } = string.Empty;
    public List<IFormFile>? Images { get; init; }
}

public sealed class UpdateArticleFormRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Visibility { get; init; }
    public int? IdCategory { get; init; }
    public string? Content { get; init; }
    public bool ReplaceImages { get; init; }
    public List<IFormFile>? Images { get; init; }
}
