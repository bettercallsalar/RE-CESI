namespace RESR.Core.Controllers.Comments;

public sealed record UpdateCommentCommand(
    int IdComment,
    string Content,
    int ActorUserId
);
