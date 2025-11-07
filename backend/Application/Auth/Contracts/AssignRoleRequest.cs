namespace Application.Auth.Contracts;

public class AssignRoleRequest
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = default!;
}