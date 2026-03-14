namespace RESR.Models.Comments;

public sealed class Comment
{
    public int IdComment { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public int IdResource { get; set; }
    public int IdUser { get; set; }
    public int? IdParentComment { get; set; }
}
