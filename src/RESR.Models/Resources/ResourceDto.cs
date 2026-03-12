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

public sealed record ArticleResponse(
    int IdResource,
    int IdArticle,
    string Title,
    string? Description,
    string Type,
    string Visibility,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    int IdUser,
    int IdCategory,
    string Content,
    bool IsApproved
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
    DateTime? CreatedTo
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
    int IdCategory,
    string? Subtitle,
    DateTime StartDate,
    DateTime? EndDate,
    string? Address,
    int? IdDepartment,
    bool IsApproved
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
