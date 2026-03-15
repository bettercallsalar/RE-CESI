namespace RESR.MAUI.Services;

public sealed record MarkedResourceItem(
    int IdResource,
    string Type,
    string Title,
    string Subtitle,
    string Summary,
    string? Route)
{
    public bool HasRoute => !string.IsNullOrWhiteSpace(Route);

    public string SearchableText => $"{Type} {Title} {Subtitle} {Summary}";
}
