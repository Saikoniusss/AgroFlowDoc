namespace Application.Worfkflow.Contracts;

public sealed class CreateProcessRequest
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public Guid DocumentTemplateId { get; set; }
    public Guid WorkflowRouteId { get; set; }
}