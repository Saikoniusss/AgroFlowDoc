public class ProcessDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public Guid DocumentTemplateId { get; set; }
    public DocumentTemplateDto DocumentTemplate { get; set; } = default!;
}