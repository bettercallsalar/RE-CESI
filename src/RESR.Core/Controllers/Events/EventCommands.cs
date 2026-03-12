using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events;

public sealed record CreateEventCommand(
    string Title,
    string? Description,
    ResourceVisibility Visibility,
    int IdUser,
    int IdCategory,
    string? Subtitle,
    DateTime StartDate,
    DateTime? EndDate,
    string? Address,
    int? IdDepartment
);

public sealed record UpdateEventCommand(
    int IdResource,
    int IdUser,
    string? Title = null,
    string? Description = null,
    ResourceVisibility? Visibility = null,
    int? IdCategory = null,
    string? Subtitle = null,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? Address = null,
    int? IdDepartment = null
);

public sealed record SetEventApprovalCommand(
    int IdResource,
    bool IsApproved
);
