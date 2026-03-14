using RESR.Models.Departments;

namespace RESR.Models.Resources;

public sealed record CreateArticleRequest(
    string Title,
    string? Description,
    string Visibility,
    int IdCategory,
    string Content
);

public sealed record UpdateArticleRequest(
    string? Title = null,
    string? Description = null,
    string? Visibility = null,
    int? IdCategory = null,
    string? Content = null
);

public sealed record SetResourceApprovalRequest(
    bool IsApproved
);

public sealed record ResourceFileResponse(
    int IdFile,
    string FileName,
    string OriginalName,
    string MimeType,
    int Size,
    string Path,
    DateTime CreatedAt
);

public sealed record ResourceAuthorResponse(
    int IdUser,
    string Username,
    string FirstName
);

public sealed record ArticleResponse(
    int IdResource,
    int IdArticle,
    string Title,
    string? Description,
    string Type,
    string Visibility,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    DateTime? DeletedAt,
    int IdUser,
    ResourceAuthorResponse Author,
    int IdCategory,
    string Content,
    bool IsApproved,
    int? DefaultImageId,
    IReadOnlyList<ResourceFileResponse> Files
);

public sealed record PaginatedArticlesResponse(
    IReadOnlyList<ArticleResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public sealed record ArticleListingFilters(
    string? Keyword,
    ResourceVisibility? Visibility,
    int? IdUser,
    int? IdCategory,
    bool? IsApproved,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    bool IncludeDeleted = false
);

public sealed record CreateEventRequest(
    string Title,
    string? Description,
    string Visibility,
    int IdCategory,
    string? Subtitle,
    DateTime StartDate,
    DateTime? EndDate,
    string? Address,
    int? IdDepartment
);

public sealed record UpdateEventRequest(
    string? Title = null,
    string? Description = null,
    string? Visibility = null,
    int? IdCategory = null,
    string? Subtitle = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Address = null,
    int? IdDepartment = null
);

public sealed record EventResponse(
    int IdResource,
    int IdEvent,
    string Title,
    string? Description,
    string Type,
    string Visibility,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    int IdUser,
    ResourceAuthorResponse Author,
    int IdCategory,
    string? Subtitle,
    DateTime StartDate,
    DateTime? EndDate,
    string? Address,
    Department? Department,
    bool IsApproved,
    IReadOnlyList<ResourceFileResponse> Files
);

public sealed record PaginatedEventsResponse(
    IReadOnlyList<EventResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public sealed record EventListingFilters(
    string? Keyword,
    ResourceVisibility? Visibility,
    int? IdUser,
    int? IdCategory,
    int? IdDepartment,
    bool? IsApproved,
    DateTime? StartFrom,
    DateTime? StartTo
);
