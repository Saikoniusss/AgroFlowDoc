namespace Domain.Entities;

public class WorkflowRouteStep
{
    public Guid Id { get; set; }
    public Guid RouteId { get; set; }
    public WorkflowRoute Route { get; set; } = default!;

    public int StepOrder { get; set; }
    public string StepName { get; set; } = default!;
    public bool IsParallel { get; set; }
    public int MinApprovals { get; set; } = 1;

    public string ApproversJson { get; set; } = "[]"; // ["role:Finance","user:..."]
}