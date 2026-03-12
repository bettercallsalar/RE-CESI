using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events.Factories;

public interface IEventFactory
{
    Event CreateFromPersistence(
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
        int? idDepartment,
        bool isApproved
    );
}
