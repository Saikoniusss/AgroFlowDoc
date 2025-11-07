namespace Application.Auth.Contracts;

public class CreateRoleRequest
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}