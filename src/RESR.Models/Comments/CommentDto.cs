namespace RESR.Models.Comments;

public sealed record CreateCommentRequest(
    string Content,
    int? IdParentComment = null
);

public sealed record UpdateCommentRequest(
    string Content
);

public sealed record CommentResponse(
    int IdComment,
    string Content,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    DateTime? DeletedAt,
    int IdResource,
    int IdUser,
    int? IdParentComment
);
