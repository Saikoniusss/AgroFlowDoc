using System.Text.Json.Serialization;

namespace Domain.Entities;
public class UserRole
{
    public Guid UserId { get; set; }
    [JsonIgnore]
    public User User { get; set; } = default!;

    public Guid RoleId { get; set; }
    [JsonIgnore]
    public Role Role { get; set; } = default!;
}