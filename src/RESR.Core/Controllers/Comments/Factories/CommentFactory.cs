using RESR.Models.Comments;

namespace RESR.Core.Controllers.Comments.Factories;

public sealed class CommentFactory : ICommentFactory
{
    public Comment CreateForCreation(
        string content,
        int idResource,
        int idUser,
        int? idParentComment
    ) =>
        new()
        {
            Content = content,
            IdResource = idResource,
            IdUser = idUser,
            IdParentComment = idParentComment
        };

    public Comment CreateFromPersistence(
        int idComment,
        string content,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? deletedAt,
        int idResource,
        int idUser,
        string? username,
        string? firstName,
        int? idParentComment
    ) =>
        new()
        {
            IdComment = idComment,
            Content = content,
            CreatedAt = createdAt,
            ModifiedAt = modifiedAt,
            DeletedAt = deletedAt,
            IdResource = idResource,
            IdUser = idUser,
            Username = username ?? string.Empty,
            FirstName = firstName ?? string.Empty,
            IdParentComment = idParentComment
        };
}
