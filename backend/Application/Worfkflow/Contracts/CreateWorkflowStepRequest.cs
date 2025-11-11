namespace Application.Worfkflow.Contracts;

public sealed class CreateWorkflowStepRequest
{
    public int StepOrder { get; set; }
    public string StepName { get; set; } = default!;
    public bool IsParallel { get; set; }
    public int MinApprovals { get; set; } = 1;
    public string ApproversJson { get; set; } = "[]";
}