namespace RESR.Models.Resources;

public enum ResourceType
{
    Article,
    Event
}

public enum ResourceVisibility
{
    PUBLIC,
    PRIVATE
}

public abstract class Resource
{
    public int IdResource { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public ResourceType Type { get; protected set; }
    public ResourceVisibility Visibility { get; set; } = ResourceVisibility.PUBLIC;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int IdUser { get; set; }
    public int IdCategory { get; set; }
}
