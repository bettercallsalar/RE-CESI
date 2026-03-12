using RESR.Models.Comments;

namespace RESR.Core.Controllers.Comments.Factories;

public interface ICommentFactory
{
    Comment CreateForCreation(
        string content,
        int idResource,
        int idUser,
        int? idParentComment
    );

    Comment CreateFromPersistence(
        int idComment,
        string content,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? deletedAt,
        int idResource,
        int idUser,
        int? idParentComment
    );
}
