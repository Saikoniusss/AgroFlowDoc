namespace Domain.Entities;

public class WFTracker
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = default!;

    public int StepOrder { get; set; }
    public string StepName { get; set; } = default!;
    public bool IsParallel { get; set; }
    public int MinApprovals { get; set; }

    public string ApproversJson { get; set; } = "[]";
    public string ApprovedByJson { get; set; } = "[]";
    public string RejectedByJson { get; set; } = "[]";

    public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected / Skipped
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}