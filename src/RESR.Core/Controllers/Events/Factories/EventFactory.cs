using RESR.Models.Departments;
using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events.Factories;

public sealed class EventFactory : IEventFactory
{
    public Event CreateFromPersistence(
        int idResource,
        int idEvent,
        string title,
        string? description,
        ResourceVisibility visibility,
        DateTime createdAt,
        DateTime? modifiedAt,
        DateTime? deletedAt,
        int idUser,
        int idCategory,
        string? subtitle,
        DateTime startDate,
        DateTime? endDate,
        string? address,
        Department? department,
        bool isApproved,
        int? defaultImageId)
    {
        return new Event
        {
            IdResource = idResource,
            IdEvent = idEvent,
            Title = title,
            Description = description,
            IsApproved = isApproved,
            Visibility = visibility,
            CreatedAt = createdAt,
            ModifiedAt = modifiedAt,
            DeletedAt = deletedAt,
            IdUser = idUser,
            IdCategory = idCategory,
            Subtitle = subtitle,
            StartDate = startDate,
            EndDate = endDate,
            Address = address,
            Department = department,
            DefaultImageId = defaultImageId
        };
    }
}
