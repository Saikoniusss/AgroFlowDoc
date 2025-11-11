using System.Text.Json.Serialization;

namespace Domain.Entities;

public class DocumentTemplateField
{
    public Guid Id { get; set; }
    public Guid TemplateId { get; set; }
    [JsonIgnore] // ← вот это ключ
    public DocumentTemplate? Template { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string Label { get; set; } = default!;
    public string FieldType { get; set; } = "text"; // text, number, date, select
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string? OptionsJson { get; set; }
}
