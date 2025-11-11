namespace Application.Worfkflow.Contracts;

public sealed class CreateWorkflowRouteRequest
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
}