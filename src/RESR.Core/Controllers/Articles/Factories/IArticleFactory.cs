using RESR.Models.Resources;

namespace RESR.Core.Controllers.Articles.Factories;

public interface IArticleFactory
{
    Article CreateFromPersistence(
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
        bool isApproved,
        int? defaultImageId
    );
}
