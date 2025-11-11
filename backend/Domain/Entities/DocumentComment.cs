namespace Domain.Entities;

public class DocumentComment
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = default!;
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string CommentText { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}