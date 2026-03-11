namespace RESR.Models.Resources;

public sealed class Event : Resource
{
    public Event()
    {
        Type = ResourceType.Event;
    }

    public int IdEvent { get; set; }
    public string? Subtitle { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Address { get; set; }
    public int? IdDepartment { get; set; }
}
