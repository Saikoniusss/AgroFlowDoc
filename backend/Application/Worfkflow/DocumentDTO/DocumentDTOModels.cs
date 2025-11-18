namespace Application.Worfkflow.DocumentDTO;

public class ProcessDetailsDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;

    public TemplateDto Template { get; set; } = default!;
    public WorkflowRouteDto WorkflowRoute { get; set; } = default!;
}

public class TemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;

    public List<TemplateFieldDto> Fields { get; set; } = new();
}

public class TemplateFieldDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Label { get; set; } = default!;
    public string FieldType { get; set; } = default!;
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public string? OptionsJson { get; set; }
}

public class WorkflowRouteDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<WorkflowStepDto> Steps { get; set; } = new();
}

public class WorkflowStepDto
{
    public Guid Id { get; set; }
    public int StepOrder { get; set; }
    public string StepName { get; set; } = default!;
    public bool IsParallel { get; set; }
    public int MinApprovals { get; set; }
    public string ApproversJson { get; set; } = default!;
}

public class CreateDocumentRequest
{
    public Guid ProcessId { get; set; }
    public string Title { get; set; } = default!;
    public Dictionary<string, object?> Fields { get; set; } = new();
}

public class DocumentDto
{
    public Guid Id { get; set; }
    public string SystemNumber { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public ProcessDto Process { get; set; } = default!;
    public string CreatedByDisplayName { get; set; } = default!;
}

public class ProcessDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
}