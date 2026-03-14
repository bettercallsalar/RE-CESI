using RESR.Core.Controllers.Resources;
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
    int? IdDepartment,
    IReadOnlyList<ResourceFileUpload>? Files = null,
    int? DefaultImageIndex = null
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
    int? IdDepartment = null,
    IReadOnlyList<ResourceFileUpload>? Files = null,
    bool ReplaceFiles = false,
    int? DefaultImageId = null,
    int? DefaultImageIndex = null
);

public sealed record SetEventApprovalCommand(
    int IdResource,
    bool IsApproved
);
