using RESR.Models.Resources;

namespace RESR.Models.Comments;

public sealed class CreateCommentRequest
{
    public CreateCommentRequest()
    {
    }

    public CreateCommentRequest(string content, int? idParentComment = null)
    {
        Content = content;
        IdParentComment = idParentComment;
    }

    public string Content { get; set; } = string.Empty;
    public int? IdParentComment { get; set; }
}

public sealed class UpdateCommentRequest
{
    public UpdateCommentRequest()
    {
    }

    public UpdateCommentRequest(string content)
    {
        Content = content;
    }

    public string Content { get; set; } = string.Empty;
}

public sealed record CommentResponse(
    int IdComment,
    string Content,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    DateTime? DeletedAt,
    int IdResource,
    int IdUser,
    ResourceAuthorResponse Author,
    int? IdParentComment
);
