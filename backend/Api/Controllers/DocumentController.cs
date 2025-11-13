using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/documents")]
public class DocumentController : ControllerBase
{
    private readonly DocflowDbContext _db;
    public DocumentController(DocflowDbContext db) => _db = db;

    [HttpGet("processes")]
    public async Task<IActionResult> GetProcesses()
    {
        var list = await _db.Processes
            .Include(p => p.DocumentTemplate) // подтягиваем шаблон
            .Include(p => p.WorkflowRoute)    // подтягиваем маршрут
            .Select(p => new 
            {
                p.Id,
                p.Name,
                DocumentTemplate = new
                {
                    p.DocumentTemplate.Id,
                    p.DocumentTemplate.Name,
                    p.DocumentTemplate.Description
                },
                WorkflowRoute = new 
                {
                    p.WorkflowRoute.Id,
                    p.WorkflowRoute.Name
                }
            })
            .ToListAsync();

        return Ok(list);
    }

    // 2. Получить шаблон для конкретного процесса
    [HttpGet("process/{processId}")]
    public async Task<IActionResult> GetProcessTemplate(Guid processId)
    {
        var process = await _db.Processes
            .Include(p => p.DocumentTemplate).ThenInclude(t => t.Fields.OrderBy(f => f.Order))
            .Include(p => p.WorkflowRoute).ThenInclude(r => r.Steps.OrderBy(s => s.StepOrder))
            .FirstOrDefaultAsync(p => p.Id == processId);

        if (process == null)
            return NotFound();

        return Ok(process);
    }

    // 3. Создать документ
    [HttpPost("create")]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest req)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // Загружаем процесс
        var process = await _db.Processes
            .Include(p => p.DocumentTemplate).ThenInclude(t => t.Fields)
            .Include(p => p.WorkflowRoute).ThenInclude(r => r.Steps)
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
            Status = "Draft"
        };

        // Создаём tracking steps
        foreach (var step in process.WorkflowRoute.Steps.OrderBy(s => s.StepOrder))
        {
            doc.WorkflowTrackers.Add(new WFTracker
            {
                Id = Guid.NewGuid(),
                DocumentId = doc.Id,
                StepOrder = step.StepOrder,
                StepName = step.StepName,
                IsParallel = step.IsParallel,
                MinApprovals = step.MinApprovals,
                ApproversJson = step.ApproversJson,
                Status = "Pending"
            });
        }

        // Устанавливаем CurrentStep
        doc.CurrentStep = doc.WorkflowTrackers.OrderBy(s => s.StepOrder).FirstOrDefault();

        _db.Documents.Add(doc);
        await _db.SaveChangesAsync();

        return Ok(new { documentId = doc.Id, systemNumber = doc.SystemNumber });
    }
}

public class CreateDocumentRequest
{
    public Guid ProcessId { get; set; }
    public string Title { get; set; } = default!;
    public string FieldsJson { get; set; } = "{}";
}