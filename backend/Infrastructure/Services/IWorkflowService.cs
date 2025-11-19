using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public interface IWorkflowService
{
    Task<List<ApproverDto>> ResolveApprovers(List<string> approvers);
}

public class WorkflowService : IWorkflowService
{
    private readonly DocflowDbContext _db;

    public WorkflowService(DocflowDbContext db)
    {
        _db = db;
    }

    public async Task<List<ApproverDto>> ResolveApprovers(List<string> approvers)
    {
        var result = new List<ApproverDto>();
        foreach (var a in approvers)
        {
            if (a.StartsWith("user:"))
            {
                var userId = Guid.Parse(a.Replace("user:", ""));
                var user = await _db.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new ApproverDto { Id = u.Id, DisplayName = u.DisplayName })
                    .FirstOrDefaultAsync();
                if (user != null)
                    result.Add(user);
            }
            else if (a.StartsWith("role:"))
            {
                var roleName = a.Replace("role:", "");
                var usersInRole = await _db.Users
                    .Where(u => u.UserRoles.Any(r => r.Role.Name == roleName))
                    .Select(u => new ApproverDto { Id = u.Id, DisplayName = u.DisplayName })
                    .ToListAsync();

                result.AddRange(usersInRole);
            }
        }
        return result;
    }
}

public class ApproverDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = default!;
}