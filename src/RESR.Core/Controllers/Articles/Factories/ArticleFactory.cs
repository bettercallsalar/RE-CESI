using RESR.Models.Resources;

namespace RESR.Core.Controllers.Articles.Factories;

public sealed class ArticleFactory : IArticleFactory
{
    public Article CreateFromPersistence(
        int idResource,
        int idArticle,
        string title,
        string? description,
        ResourceVisibility visibility,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? deletedAt,
        int idUser,
        int idCategory,
        string content,
        bool isApproved)
    {
        return new Article
        {
            IdResource = idResource,
            IdArticle = idArticle,
            Title = title,
            Description = description,
            IsApproved = isApproved,
            Visibility = visibility,
            CreatedAt = createdAt,
            ModifiedAt = modifiedAt,
            DeletedAt = deletedAt,
            IdUser = idUser,
            IdCategory = idCategory,
            Content = content
        };
    }
}
