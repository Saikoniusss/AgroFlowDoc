namespace Domain.Entities;

public class DocumentFile
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = default!;

    public string FileName { get; set; } = default!;
    public string RelativePath { get; set; } = default!;
    public string ContentType { get; set; } = default!;

    public Guid UploadedById { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
