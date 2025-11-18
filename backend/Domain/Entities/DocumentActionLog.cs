using System.Text.Json.Serialization;

namespace Domain.Entities;

public class DocumentActionLog
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    [JsonIgnore] // ← вот это ключ
    public Document Document { get; set; } = default!;
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
    public string ActionType { get; set; } = default!; // Просмотр, Редактирование, Утверждение, Отклонение, Комментарий, Добавление файла
    public string? Details { get; set; }
    public DateTime ActionAtUtc { get; set; } = DateTime.UtcNow;
}