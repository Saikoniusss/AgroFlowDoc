namespace Domain.Entities;

public class Process
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }

    public Guid DocumentTemplateId { get; set; }
    public DocumentTemplate DocumentTemplate { get; set; } = default!;

    public Guid WorkflowRouteId { get; set; }
    public WorkflowRoute WorkflowRoute { get; set; } = default!;

    public bool IsActive { get; set; } = true;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}