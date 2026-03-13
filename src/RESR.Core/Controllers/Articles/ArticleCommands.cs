using RESR.Core.Controllers.Resources;
using RESR.Models.Resources;

namespace RESR.Core.Controllers.Articles;

public sealed record CreateArticleCommand(
    string Title,
    string? Description,
    ResourceVisibility Visibility,
    int IdUser,
    int IdCategory,
    string Content,
    IReadOnlyList<ResourceFileUpload>? Files = null
);

public sealed record UpdateArticleCommand(
    int IdResource,
    int IdUser,
    string? Title = null,
    string? Description = null,
    ResourceVisibility? Visibility = null,
    int? IdCategory = null,
    string? Content = null,
    IReadOnlyList<ResourceFileUpload>? Files = null,
    bool ReplaceFiles = false
);

public sealed record SetArticleApprovalCommand(
    int IdResource,
    bool IsApproved
);
