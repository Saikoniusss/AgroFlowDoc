namespace Application.Worfkflow.Contracts;

public sealed class UpdateFieldRequest
{
    public string Name { get; set; } = default!;
    public string Label { get; set; } = default!;
    public string FieldType { get; set; } = "text"; // text, number, date, select, bool
    public bool IsRequired { get; set; }
    public int Order { get; set; } = 1;
    public string? OptionsJson { get; set; }
}