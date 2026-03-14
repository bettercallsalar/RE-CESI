using Microsoft.AspNetCore.Http;

namespace RESR.WebAPI.Routes.Events;

public sealed class CreateEventFormRequest
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Visibility { get; init; } = "PUBLIC";
    public int IdCategory { get; init; }
    public string? Subtitle { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Address { get; init; }
    public int? IdDepartment { get; init; }
    public List<IFormFile>? Images { get; init; }
}

public sealed class UpdateEventFormRequest
{
    public string? Title { get; init; }
    public string? Description { get; init; }
    public string? Visibility { get; init; }
    public int? IdCategory { get; init; }
    public string? Subtitle { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string? Address { get; init; }
    public int? IdDepartment { get; init; }
    public bool ReplaceImages { get; init; }
    public List<IFormFile>? Images { get; init; }
}
