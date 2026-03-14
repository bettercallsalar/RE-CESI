namespace RESR.Models.Marks;

public sealed record CreateMarkRequest(
    bool IsFavorite,
    bool IsReadLater,
    int IdRessource
);

public sealed record UpdateMarkRequest(
    bool IsFavorite,
    bool IsReadLater,
    int IdRessource
);

public sealed record MarkResponse(
    int IdMark,
    bool IsFavorite,
    bool IsReadLater,
    int IdRessource,
    int IdUser
);

public sealed record PaginatedMarksResponse(
    IReadOnlyList<MarkResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);
