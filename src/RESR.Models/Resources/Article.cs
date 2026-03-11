namespace RESR.Models.Resources;

public sealed class Article : Resource
{
    public Article()
    {
        Type = ResourceType.Article;
    }

    public int IdArticle { get; set; }
    public required string Content { get; set; }
    public bool IsApproved { get; set; }
}
