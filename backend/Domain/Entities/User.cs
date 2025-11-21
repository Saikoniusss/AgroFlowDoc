namespace Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? TelegramUsername { get; set; }
    public long? TelegramChatId { get; set; } 
    public string? AvatarPath { get; set; }  // ← ДОБАВЛЕНО

    public bool IsApproved { get; set; } = false; // админ должен подтвердить
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
     public DateTime? UpdatedAtUtc { get; set; }
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}