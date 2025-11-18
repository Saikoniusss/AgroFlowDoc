namespace Domain.Entities;

public class Document
{
    public Guid Id { get; set; }
    public string SystemNumber { get; set; } = default!;
    public Guid ProcessId { get; set; }
    public Process Process { get; set; } = default!;
    public Guid TemplateId { get; set; }
    public DocumentTemplate Template { get; set; } = default!;
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = default!;
    public Guid? CurrentStepId { get; set; }
    public WFTracker? CurrentStep { get; set; }

    public string Title { get; set; } = default!;
    public string FieldsJson { get; set; } = "{}";
    public string Status { get; set; } = "Черновик"; // Черновик / На согласовании / Утверждено / Отклонено

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<WFTracker> WorkflowTrackers { get; set; } = new List<WFTracker>();
    public ICollection<DocumentFile> Files { get; set; } = new List<DocumentFile>();
    public ICollection<DocumentComment> Comments { get; set; } = new List<DocumentComment>();
    public ICollection<DocumentActionLog> Actions { get; set; } = new List<DocumentActionLog>();
}