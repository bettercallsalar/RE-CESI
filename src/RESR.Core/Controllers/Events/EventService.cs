using RESR.Core.Controllers.Events.Ports;
using RESR.Core.Errors;
using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events;

public sealed class EventService : IEventService
{
    private readonly IEventRepository _repo;

    public EventService(IEventRepository repo) => _repo = repo;

    public Task<IReadOnlyList<Event>> GetAllAsync(CancellationToken ct) => _repo.GetAllAsync(ct);

    public Task<Event?> GetByResourceIdAsync(int idResource, CancellationToken ct) => _repo.GetByResourceIdAsync(idResource, ct);

    public async Task<int> CreateAsync(CreateEventCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Title))
            throw new ValidationException("Title is required.");
        if (cmd.IdUser <= 0)
            throw new ValidationException("IdUser must be greater than 0.");
        if (cmd.IdCategory <= 0)
            throw new ValidationException("IdCategory must be greater than 0.");
        if (cmd.IdDepartment is <= 0)
            throw new ValidationException("IdDepartment must be greater than 0.");
        if (cmd.EndDate is not null && cmd.EndDate < cmd.StartDate)
            throw new ValidationException("EndDate cannot be earlier than StartDate.");

        var normalized = cmd with
        {
            Title = cmd.Title.Trim(),
            Description = NormalizeOptional(cmd.Description),
            Subtitle = NormalizeOptional(cmd.Subtitle),
            Address = NormalizeOptional(cmd.Address)
        };

        return await _repo.CreateAsync(normalized, ct);
    }

    public async Task<Event> UpdateAsync(UpdateEventCommand cmd, CancellationToken ct)
    {
        if (cmd.IdResource <= 0)
            throw new ValidationException("IdResource must be greater than 0.");

        var existing = await _repo.GetByResourceIdAsync(cmd.IdResource, ct);
        if (existing is null)
            throw new NotFoundException($"Event resource {cmd.IdResource} not found.");

        if (cmd.IdCategory is <= 0)
            throw new ValidationException("IdCategory must be greater than 0.");
        if (cmd.IdDepartment is <= 0)
            throw new ValidationException("IdDepartment must be greater than 0.");

        var effectiveStartDate = cmd.StartDate ?? existing.StartDate;
        var effectiveEndDate = cmd.EndDate ?? existing.EndDate;
        if (effectiveEndDate is not null && effectiveEndDate < effectiveStartDate)
            throw new ValidationException("EndDate cannot be earlier than StartDate.");

        var normalized = cmd with
        {
            Title = NormalizeOptional(cmd.Title),
            Description = NormalizeOptional(cmd.Description),
            Subtitle = NormalizeOptional(cmd.Subtitle),
            Address = NormalizeOptional(cmd.Address)
        };

        return await _repo.PatchAsync(normalized, ct)
            ?? throw new NotFoundException($"Event resource {cmd.IdResource} not found.");
    }

    public Task<bool> SoftDeleteAsync(int idResource, CancellationToken ct) => _repo.SoftDeleteAsync(idResource, ct);

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }
}
