using RESR.Core.Controllers.Events.Ports;
using RESR.Core.Controllers.Resources.Ports;
using RESR.Core.Errors;
using RESR.Models.Resources;

namespace RESR.Core.Controllers.Events;

public sealed class EventService : IEventService
{
    private sealed class NullResourceFileRepository : IResourceFileRepository
    {
        public Task<IReadOnlyDictionary<int, IReadOnlyList<ResourceFile>>> GetByResourceIdsAsync(IReadOnlyCollection<int> resourceIds, CancellationToken ct) =>
            Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<ResourceFile>>>(new Dictionary<int, IReadOnlyList<ResourceFile>>());

        public Task ReplaceForResourceAsync(int idResource, IReadOnlyList<ResourceFile> files, CancellationToken ct) => Task.CompletedTask;

        public Task DeleteForResourceAsync(int idResource, CancellationToken ct) => Task.CompletedTask;
    }

    private sealed class NullResourceFileStorage : IResourceFileStorage
    {
        public Task<IReadOnlyList<ResourceFile>> SaveAsync(int idResource, int idUser, IReadOnlyList<Core.Controllers.Resources.ResourceFileUpload> uploads, CancellationToken ct) =>
            Task.FromResult<IReadOnlyList<ResourceFile>>(Array.Empty<ResourceFile>());

        public Task DeleteAsync(IReadOnlyList<ResourceFile> files, CancellationToken ct) => Task.CompletedTask;
    }

    private readonly IEventRepository _repo;
    private readonly IResourceFileRepository _fileRepository;
    private readonly IResourceFileStorage _fileStorage;

    public EventService(IEventRepository repo)
        : this(repo, new NullResourceFileRepository(), new NullResourceFileStorage())
    {
    }

    public EventService(IEventRepository repo, IResourceFileRepository fileRepository, IResourceFileStorage fileStorage)
    {
        _repo = repo;
        _fileRepository = fileRepository;
        _fileStorage = fileStorage;
    }

    public async Task<(IReadOnlyList<Event> Events, int TotalCount)> GetPaginatedAsync(
        int page,
        int pageSize,
        EventListingFilters filters,
        CancellationToken ct)
    {
        var normalizedFilters = NormalizeListingFilters(filters);
        var events = await _repo.GetPaginatedAsync(page, pageSize, normalizedFilters, ct);
        await AttachFilesAsync(events, ct);
        var totalCount = await _repo.CountAsync(normalizedFilters, ct);
        return (events, totalCount);
    }

    public async Task<Event?> GetByResourceIdAsync(int idResource, CancellationToken ct)
    {
        var @event = await _repo.GetByResourceIdAsync(idResource, ct);

        if (@event is null)
            return null;

        await AttachFilesAsync(new[] { @event }, ct);
        return @event;
    }

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

        ValidateFiles(normalized.Files);

        var idResource = await _repo.CreateAsync(normalized, ct);

        if (normalized.Files is { Count: > 0 })
        {
            var storedFiles = await _fileStorage.SaveAsync(idResource, normalized.IdUser, normalized.Files, ct);
            await _fileRepository.ReplaceForResourceAsync(idResource, storedFiles, ct);
        }

        return idResource;
    }

    public async Task<Event> UpdateAsync(UpdateEventCommand cmd, CancellationToken ct)
    {
        var existingEvent = await _repo.GetByResourceIdAsync(cmd.IdResource, ct) ?? throw new NotFoundException($"Event resource {cmd.IdResource} not found.");
        if (existingEvent?.IdUser != cmd.IdUser)
            throw new ForbiddenException("You do not have permission to update this event.");

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

        ValidateFiles(normalized.Files);

        var updatedEvent = await _repo.PatchAsync(normalized, ct)
            ?? throw new NotFoundException($"Event resource {cmd.IdResource} not found.");

        if (normalized.ReplaceFiles)
        {
            var existingFiles = await _fileRepository.GetByResourceIdsAsync(new[] { cmd.IdResource }, ct);

            if (existingFiles.TryGetValue(cmd.IdResource, out var filesToDelete) && filesToDelete.Count > 0)
                await _fileStorage.DeleteAsync(filesToDelete, ct);

            var storedFiles = normalized.Files is { Count: > 0 }
                ? await _fileStorage.SaveAsync(cmd.IdResource, cmd.IdUser, normalized.Files, ct)
                : Array.Empty<ResourceFile>();

            await _fileRepository.ReplaceForResourceAsync(cmd.IdResource, storedFiles, ct);
            return await _repo.GetByResourceIdAsync(cmd.IdResource, ct)
                ?? throw new NotFoundException($"Event resource {cmd.IdResource} not found.");
        }

        return updatedEvent;
    }

    public async Task<Event> SetApprovalAsync(SetEventApprovalCommand cmd, CancellationToken ct)
    {
        if (cmd.IdResource <= 0)
            throw new ValidationException("IdResource must be greater than 0.");

        return await _repo.SetApprovalAsync(cmd, ct)
            ?? throw new NotFoundException($"Event resource {cmd.IdResource} not found.");
    }

    public async Task<bool> SoftDeleteAsync(int idResource, int idUser, CancellationToken ct)
    {
        var existingEvent = await _repo.GetByResourceIdAsync(idResource, ct) ?? throw new NotFoundException($"Event resource {idResource} not found.");
        if (existingEvent?.IdUser != idUser)
            throw new ForbiddenException("You do not have permission to delete this event.");

        var deleted = await _repo.SoftDeleteAsync(idResource, ct);

        if (deleted)
        {
            var existingFiles = await _fileRepository.GetByResourceIdsAsync(new[] { idResource }, ct);

            if (existingFiles.TryGetValue(idResource, out var files) && files.Count > 0)
            {
                await _fileStorage.DeleteAsync(files, ct);
                await _fileRepository.DeleteForResourceAsync(idResource, ct);
            }
        }

        return deleted;
    }

    private static string? NormalizeOptional(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }

    private static EventListingFilters NormalizeListingFilters(EventListingFilters filters)
    {
        return filters with
        {
            Keyword = NormalizeOptional(filters.Keyword)
        };
    }

    private static void ValidateFiles(IReadOnlyList<Core.Controllers.Resources.ResourceFileUpload>? files)
    {
        if (files is null || files.Count == 0)
            return;

        if (files.Count > 6)
            throw new ValidationException("Vous ne pouvez pas envoyer plus de 6 images.");

        foreach (var file in files)
        {
            if (file.Size <= 0)
                throw new ValidationException("Une image envoyee est vide.");

            if (file.Size > 5 * 1024 * 1024)
                throw new ValidationException("Chaque image doit faire moins de 5 Mo.");

            if (!file.MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("Seules les images sont autorisees.");
        }
    }

    private async Task AttachFilesAsync(IReadOnlyList<Event> events, CancellationToken ct)
    {
        if (events.Count == 0)
            return;

        var filesByResource = await _fileRepository.GetByResourceIdsAsync(events.Select(@event => @event.IdResource).ToArray(), ct);

        foreach (var @event in events)
            @event.Files = filesByResource.TryGetValue(@event.IdResource, out var files) ? files : Array.Empty<ResourceFile>();
    }
}
