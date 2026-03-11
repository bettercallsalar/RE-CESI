using RESR.Models.Resources;

namespace RESR.Core.Controllers.Articles;

public sealed record CreateArticleCommand(
    string Title,
    string? Description,
    ResourceVisibility Visibility,
    int IdUser,
    int IdCategory,
    string Content
);

public sealed record UpdateArticleCommand(
    int IdResource,
    string? Title = null,
    string? Description = null,
    ResourceVisibility? Visibility = null,
    int? IdCategory = null,
    string? Content = null
);

public sealed record SetArticleApprovalCommand(
    int IdResource,
    bool IsApproved
);
