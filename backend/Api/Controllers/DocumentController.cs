using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.Worfkflow.DocumentDTO;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/documents")]
public class DocumentController : ControllerBase
{
    private readonly DocflowDbContext _db;
    public DocumentController(DocflowDbContext db) 
    {
        _db = db; 
    }

     // 1. Список процессов (для списка "Создать документ")
    [HttpGet("processes")]
    public async Task<IActionResult> GetProcesses()
    {
        var result = await _db.Processes
            .Include(p => p.DocumentTemplate)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Code,
                TemplateName = p.DocumentTemplate.Name
            })
            .ToListAsync();

        return Ok(result);
    }

    // 2. Детали процесса для формы создания
    [HttpGet("process/{id:guid}")]
    public async Task<IActionResult> GetProcessDetails(Guid id)
    {
        var p = await _db.Processes
            .Include(p => p.DocumentTemplate)
                .ThenInclude(t => t.Fields)
            .Include(p => p.WorkflowRoute)
                .ThenInclude(r => r.Steps)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (p == null) return NotFound();

        var dto = new ProcessDetailsDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            Template = new TemplateDto
            {
                Id = p.DocumentTemplate.Id,
                Name = p.DocumentTemplate.Name,
                Code = p.DocumentTemplate.Code,
                Fields = p.DocumentTemplate.Fields
                    .OrderBy(f => f.Order)
                    .Select(f => new TemplateFieldDto
                    {
                        Id = f.Id,
                        Name = f.Name,
                        Label = f.Label,
                        FieldType = f.FieldType,
                        IsRequired = f.IsRequired,
                        Order = f.Order,
                        OptionsJson = f.OptionsJson
                    })
                    .ToList()
            },
            WorkflowRoute = new WorkflowRouteDto
            {
                Id = p.WorkflowRoute.Id,
                Name = p.WorkflowRoute.Name,
                Steps = p.WorkflowRoute.Steps
                    .OrderBy(s => s.StepOrder)
                    .Select(s => new WorkflowStepDto
                    {
                        Id = s.Id,
                        StepOrder = s.StepOrder,
                        StepName = s.StepName,
                        IsParallel = s.IsParallel,
                        MinApprovals = s.MinApprovals,
                        ApproversJson = s.ApproversJson
                    })
                    .ToList()
            }
        };

        return Ok(dto);
    }

    // 3. Создать документ
    [HttpPost("create")]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Загружаем процесс
        var process = await _db.Processes
            .Include(p => p.DocumentTemplate).ThenInclude(t => t.Fields)
            .Include(p => p.WorkflowRoute).ThenInclude(r => r.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(p => p.Id == req.ProcessId);

        if (process == null)
            return NotFound("Process not found");

        // Генерируем системный номер
        string sysNo = $"DOC-{DateTime.UtcNow:yyyyMMddHHmmss}-{new Random().Next(1000, 9999)}";

        var doc = new Document
        {
            Id = Guid.NewGuid(),
            SystemNumber = sysNo,
            ProcessId = process.Id,
            TemplateId = process.DocumentTemplateId,
            CreatedById = userId,
            Title = req.Title,
            FieldsJson = req.FieldsJson,
            Status = req.Submit ? "InProgress" : "Draft",
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        // Создаём tracking steps
        if (req.Submit)
        {
            foreach (var step in process.WorkflowRoute!.Steps.OrderBy(x => x.StepOrder))
            {
                var tracker = new WFTracker
                {
                    Id = Guid.NewGuid(),
                    DocumentId = doc.Id,
                    StepOrder = step.StepOrder,
                    StepName = step.StepName,
                    IsParallel = step.IsParallel,
                    MinApprovals = step.MinApprovals,
                    ApproversJson = step.ApproversJson,
                    Status = step.StepOrder == 1 ? "Pending" : "Waiting"
                };

                _db.WFTrackers.Add(tracker);

                if (step.StepOrder == 1)
                    doc.CurrentStepId = tracker.Id;
            }
        }
         await _db.SaveChangesAsync();
    

        return Ok(new
        {
            message = req.Submit ? "Документ отправлен на согласование" : "Черновик сохранён",
            documentId = doc.Id
        });
    }
}

public class CreateDocumentRequest
{
    public Guid ProcessId { get; set; }
    public string Title { get; set; } = default!;
    public string FieldsJson { get; set; } = "{}";
    public bool Submit { get; set; } // true = отправить, false = сохранить draft
}