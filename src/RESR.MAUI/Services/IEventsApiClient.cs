using RESR.Models.Resources;

namespace RESR.MAUI.Services;

public interface IEventsApiClient
{
    Task<EventResponse> GetByIdAsync(int idResource, CancellationToken ct);
    Task<EventResponse> GetOwnByIdAsync(int idResource, CancellationToken ct);
    Task CreateAsync(
        CreateEventRequest request,
        IReadOnlyList<SelectedImageUpload> images,
        int? defaultImageIndex,
        CancellationToken ct);
    Task UpdateAsync(
        int idResource,
        UpdateEventRequest request,
        IReadOnlyList<SelectedImageUpload> images,
        int? defaultImageIndex,
        CancellationToken ct);
}
