namespace Domain.Entities;

public class DocumentTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }

    public ICollection<DocumentTemplateField> Fields { get; set; } = new List<DocumentTemplateField>();
    public ICollection<Process> Processes { get; set; } = new List<Process>();
}
