using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.Worfkflow.DocumentDTO;
using System.Text.Json;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize]
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
    [HttpPost("{documentId}/files/upload")]
    public async Task<IActionResult> UploadFile(Guid documentId, IFormFile file)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var document = await _db.Documents.FindAsync(documentId);
        if (document == null)
            return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest("Empty file");

        // Папка хранения (лучше вынести в конфиг)
        var basePath = Path.Combine("uploads", documentId.ToString());
        Directory.CreateDirectory(basePath);

        var filePath = Path.Combine(basePath, file.FileName);

        using (var stream = System.IO.File.Create(filePath))
            await file.CopyToAsync(stream);

        var entity = new DocumentFile
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            FileName = file.FileName,
            RelativePath = filePath.Replace("\\", "/"),
            ContentType = file.ContentType,
            UploadedById = userId,
            UploadedAtUtc = DateTime.UtcNow
        };

        _db.DocumentFiles.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Файл загружен", file = entity });
    }
    [HttpGet("my")]
    public async Task<IActionResult> GetMyDocuments()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var docs = await _db.Documents
            .Where(d => d.CreatedById == userId)
            .OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => new
            {
                d.Id,
                d.SystemNumber,
                d.Title,
                d.Status,
                d.CreatedAtUtc,
                Process = new
                {
                    d.Process.Id,
                    d.Process.Name,
                    d.Process.Code
                }
            })
            .ToListAsync();
        return Ok(docs);
    }
    [HttpGet("todo")]
    public async Task<IActionResult> GetDocumentsForApproval()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
            return Unauthorized("User not found in token");

        var userId = Guid.Parse(userIdString);
        var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        var trackers = await _db.WFTrackers
            .Include(t => t.Document).ThenInclude(d => d.Process)
            .Where(t => t.Status == "Pending")
            .ToListAsync();

        var available = trackers
            .Where(t =>
            {
                var approvers = JsonSerializer.Deserialize<List<string>>(t.ApproversJson ?? "[]") ?? new();

                bool byUser = approvers.Contains($"user:{userId}");
                bool byRole = userRoles.Any(role => approvers.Contains($"role:{role}"));

                return byUser || byRole;
            })
            .Select(t => t.Document)
            .Distinct()
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToList();

        return Ok(available);
    }
    [HttpGet("archive")]
    public async Task<IActionResult> GetArchive()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var trackers = await _db.WFTrackers
            .Include(t => t.Document).ThenInclude(d => d.Process)
            .Where(t =>
                t.ApproversJson.Contains($"user:{userId}") &&
                t.Status != "Pending"
            )
            .ToListAsync();

        var docs = trackers
            .Select(t => t.Document)
            .Distinct()
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToList();

        return Ok(docs);
    }
    [HttpGet("menu-counts")]
    public async Task<IActionResult> GetMenuCounts()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userRoles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        var processes = await _db.Processes
            .Include(p => p.DocumentTemplate)
            .ToListAsync();

        var results = new List<object>();

        foreach (var p in processes)
        {
            var myCount = await _db.Documents
                .CountAsync(d => d.ProcessId == p.Id && d.CreatedById == userId);

            var todoTrackers = await _db.WFTrackers
                .Where(t => t.Document.ProcessId == p.Id && t.Status == "Pending")
                .ToListAsync();

            var todoCount = todoTrackers.Count(t =>
            {
                var approvers = JsonSerializer.Deserialize<List<string>>(t.ApproversJson);
                return approvers.Contains($"user:{userId}")
                    || userRoles.Any(r => approvers.Contains($"role:{r}"));
            });

            var archivedCount = await _db.WFTrackers
                .CountAsync(t =>
                    t.Document.ProcessId == p.Id &&
                    t.ApproversJson.Contains($"user:{userId}") &&
                    t.Status != "Pending"
                );

            results.Add(new
            {
                processId = p.Id,
                processName = p.Name,
                my = myCount,
                todo = todoCount,
                archive = archivedCount
            });
        }

        return Ok(results);
    }
}

public class CreateDocumentRequest
{
    public Guid ProcessId { get; set; }
    public string Title { get; set; } = default!;
    public string FieldsJson { get; set; } = "{}";
    public bool Submit { get; set; } // true = отправить, false = сохранить draft
}