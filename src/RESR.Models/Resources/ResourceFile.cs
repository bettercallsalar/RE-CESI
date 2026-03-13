namespace RESR.Models.Resources;

public sealed class ResourceFile
{
    public int IdFile { get; set; }
    public required string FileName { get; set; }
    public required string OriginalName { get; set; }
    public required string MimeType { get; set; }
    public int Size { get; set; }
    public required string Path { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public int IdResource { get; set; }
}
