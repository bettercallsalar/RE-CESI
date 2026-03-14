namespace RESR.Core.Controllers.Comments;

public sealed record CreateCommentCommand(
    int IdResource,
    string Content,
    int IdUser,
    int? IdParentComment
);
