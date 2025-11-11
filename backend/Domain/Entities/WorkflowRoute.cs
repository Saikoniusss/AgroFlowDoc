namespace Domain.Entities;

public class WorkflowRoute
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }

    public ICollection<WorkflowRouteStep> Steps { get; set; } = new List<WorkflowRouteStep>();
    public ICollection<Process> Processes { get; set; } = new List<Process>();
}