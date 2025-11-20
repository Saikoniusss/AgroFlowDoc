using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Application.Worfkflow.DocumentDTO;
using System.Text.Json;
using Application.Worfkflow;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize]
public class DocumentController : ControllerBase
{
    private readonly DocflowDbContext _db;
    private readonly IWorkflowService _workflowService;
    private readonly ILogger<DocumentController> _logger;
    private readonly IConfiguration _config;
    public DocumentController(DocflowDbContext db, IWorkflowService workflowService, ILogger<DocumentController> logger, IConfiguration configuration) 
    {
        _db = db; 
        _workflowService = workflowService;
        _logger = logger;
        _config = configuration;
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
            Status = req.Submit ? "Согласование" : "Черновик",
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

        // Папка хранения (можно брать из appsettings.json)
          // Абсолютный путь к папке
        var uploadRoot = _config.GetValue<string>("FileStorage:BasePath")
                     ?? Path.Combine(Directory.GetCurrentDirectory(), "DocumentUploads");
        if (!Path.IsPathRooted(uploadRoot))
        {
            uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), uploadRoot);
        }

        Directory.CreateDirectory(uploadRoot);
         // Генерируем уникальное имя файла
        var fileGuid = Guid.NewGuid();
        var extension = Path.GetExtension(file.FileName);
        var savedFileName = fileGuid + extension;

        var filePath = Path.Combine(uploadRoot, savedFileName);

        try
        {
            await using var stream = System.IO.File.Create(filePath);
            await file.CopyToAsync(stream);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Ошибка сохранения файла: {ex.Message}");
        }

        var entity = new DocumentFile
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            FileName = file.FileName,
            RelativePath = savedFileName,
            ContentType = file.ContentType,
            UploadedById = userId,
            UploadedAtUtc = DateTime.UtcNow
        };

        _db.DocumentFiles.Add(entity);
        await _db.SaveChangesAsync();

        return Ok(new 
        {
            success = true,
            fileId = entity.Id,
            savedFileName
        });
    }

    [HttpGet("files/{fileId}/download")]
    public async Task<IActionResult> DownloadFile(Guid fileId)
    {
        var fileRecord = await _db.DocumentFiles.FindAsync(fileId);

        if (fileRecord == null)
            return NotFound("Файл не найден в базе");

        var basePath = _config.GetValue<string>("FileStorage:BasePath")!;
        var fullPath = Path.Combine(basePath, fileRecord.RelativePath);

        if (!System.IO.File.Exists(fullPath))
            return NotFound("Файл отсутствует на диске");

        var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);

        return File(
            fileBytes,
            fileRecord.ContentType,
            fileRecord.FileName,                 // 👈 отдаём оригинальное имя
            enableRangeProcessing: true          // 👈 позволяет докачку
        );
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyDocuments()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var docs = await _db.Documents
            .Where(d => d.CreatedById == userId)
            .Include(d => d.CreatedBy)
            .OrderByDescending(d => d.CreatedAtUtc)
            .Select(d => new DocumentDto
            {
                Id =d.Id,
                SystemNumber = d.SystemNumber,
                Title = d.Title,
                Status = d.Status,
                CreatedAtUtc = d.CreatedAtUtc,
                Process = new Application.Worfkflow.DocumentDTO.ProcessDto
                {
                    Id = d.Process.Id,
                    Name = d.Process.Name,
                    Code = d.Process.Code
                },
                CreatedByDisplayName = d.CreatedBy.DisplayName
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
            .Include(t => t.Document)
            .ThenInclude(d => d.CreatedBy) // <-- добавили
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
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                SystemNumber = d.SystemNumber,
                Title = d.Title,
                Status = d.Status,
                CreatedAtUtc = d.CreatedAtUtc,
                CreatedByDisplayName = d.CreatedBy.DisplayName,
                Process = new Application.Worfkflow.DocumentDTO.ProcessDto
                {
                    Id = d.Process.Id,
                    Name = d.Process.Name,
                    Code = d.Process.Code
                }
            })
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
            .Select(d => new DocumentDto
            {
                Id = d.Id,
                SystemNumber = d.SystemNumber,
                Title = d.Title,
                Status = d.Status,
                CreatedAtUtc = d.CreatedAtUtc,
                CreatedByDisplayName = d.CreatedBy.DisplayName,
                Process = new Application.Worfkflow.DocumentDTO.ProcessDto
                {
                    Id = d.Process.Id,
                    Name = d.Process.Name,
                    Code = d.Process.Code
                }
            })
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


    // 📄 ПРОСМОТР ДОКУМЕНТА
    // ----------------------------------------------------------------
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> View(Guid id)
    {
        var doc = await _db.Documents
            .Include(d => d.Template).ThenInclude(t => t.Fields)
            .Include(d => d.Process)
            .Include(d => d.Files)
            .Include(d => d.CreatedBy)
            .Include(d => d.WorkflowTrackers.OrderBy(t => t.StepOrder))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
            return NotFound();

        // ----------------------------------------------------
        // 🧠 Текущий пользователь из токена
        // ----------------------------------------------------
        Guid currentUserId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        // ----------------------------------------------------
        // 🧠 Проверяем — может ли он утверждать документ
        // ----------------------------------------------------
        bool canApprove = false;

        var currentStep = doc.WorkflowTrackers.FirstOrDefault(t => t.Id == doc.CurrentStepId);

        if (currentStep != null)
        {
            var approversList = JsonSerializer.Deserialize<List<string>>(currentStep.ApproversJson ?? "[]") ?? new();
            foreach (var rule in approversList)
            {
                if (rule.StartsWith("user:") && rule.Substring(5) == currentUserId.ToString())
                    canApprove = true;
                else if (rule.StartsWith("role:") && roles.Contains(rule.Substring(5)))
                    canApprove = true;
            }
        }
        var workflowDtos = new List<object>();
        foreach (var t in doc.WorkflowTrackers.OrderBy(t => t.StepOrder))
        {
            // Асинхронно получаем списки исполнителей
            var approvers = await SafeResolveApprovers(
                JsonSerializer.Deserialize<List<string>>(t.ApproversJson ?? "[]") ?? new()
            );

            var approvedBy = await SafeResolveApprovers(
                JsonSerializer.Deserialize<List<string>>(t.ApprovedByJson ?? "[]") ?? new()
            );

            var rejectedBy = await SafeResolveApprovers(
                JsonSerializer.Deserialize<List<string>>(t.RejectedByJson ?? "[]") ?? new()
            );
            workflowDtos.Add(new
            {
                t.Id,
                t.StepOrder,
                t.StepName,
                t.IsParallel,
                t.MinApprovals,
                t.Status,
                t.CompletedAtUtc,

                Approvers = approvers,
                ApprovedBy = approvedBy,
                RejectedBy = rejectedBy,

                Executors = t.CompletedAtUtc == null ? approvers : t.Status == "Rejected" ? rejectedBy : approvedBy
            });
        }

        var result = new
        {
            doc.Id,
            doc.SystemNumber,
            doc.Title,
            doc.Status,
            doc.CreatedAtUtc,
            doc.UpdatedAtUtc,
            DisplayName = doc.CreatedBy.DisplayName,
            Process = new { doc.Process.Id, doc.Process.Name },

            Template = new
            {
                doc.Template.Id,
                doc.Template.Name,
                Fields = doc.Template.Fields.OrderBy(f => f.Order)
            },

            FieldsJson = doc.FieldsJson,

            Files = doc.Files.Select(f => new
            {
                f.Id,
                f.FileName,
                f.RelativePath,
                f.ContentType
            }),
            
            Workflow = workflowDtos,
            CanApprove = canApprove
        };

        return Ok(result);
    }

    private async Task<List<ApproverDto>> SafeResolveApprovers(List<string> input)
    {
        try
        {
            return await _workflowService.ResolveApprovers(input);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка ResolveApprovers");
            return new List<ApproverDto>();
        }
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        List<string> approvers;
        var doc = await _db.Documents
            .Include(d => d.Process)
            .Include(d => d.WorkflowTrackers.OrderBy(t => t.StepOrder))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
            return NotFound();

        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var current = doc.WorkflowTrackers.FirstOrDefault(t => t.Id == doc.CurrentStepId);
        if (current == null)
            return BadRequest("У документа нет активного этапа.");

        // --- Проверка: может ли пользователь согласовать ---
        var routeStep = await _db.WorkflowRouteSteps.FirstOrDefaultAsync(s =>
            s.RouteId == doc.Process.WorkflowRouteId &&
            s.StepOrder == current.StepOrder);

        if (routeStep == null)
            return BadRequest("Маршрут повреждён.");
        
        try
        {
            approvers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(routeStep.ApproversJson ?? "[]")    
            ?? new List<string>(); 
        } catch (Exception ex)
        {
            return BadRequest("Ошибка ApproversJson: " + ex.Message);
        }
            
        bool allowed = false;
        foreach (var ap in approvers)
        {
            if (ap.StartsWith("user:"))
            {
                if (ap.Replace("user:", "") == userId.ToString())
                    allowed = true;
            }

            if (ap.StartsWith("role:"))
            {
                var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
                if (roles.Contains(ap.Replace("role:", "")))
                    allowed = true;
            }
        }

        if (!allowed)
            return Forbid();

        // --- Утверждаем текущий шаг ---
        current.Status = "Approved";
        current.CompletedAtUtc = DateTime.UtcNow;
        current.ApprovedByJson = $"[\"user:{userId}\"]";

        await _db.SaveChangesAsync();

        // ========== Параллельное согласование ==========
        var sameOrderSteps = doc.WorkflowTrackers.Where(t => t.StepOrder == current.StepOrder).ToList();

        bool allApproved = sameOrderSteps.All(t => t.Status == "Approved");

        if (allApproved)
        {
            var nextStep = doc.WorkflowTrackers
                .Where(t => t.StepOrder > current.StepOrder)
                .OrderBy(t => t.StepOrder)
                .FirstOrDefault();

            if (nextStep == null)
            {
                // 🎉 Финальный шаг → документ утверждён
                doc.Status = "Утверждён";
                doc.CurrentStepId = null;
            }
            else
            {
                doc.Status = "На согласовании";
                doc.CurrentStepId = nextStep.Id;
                nextStep.Status = "Pending";
                nextStep.StartedAtUtc = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Документ утверждён" });
    }

    public class RejectRequest
    {
        public string Comment { get; set; } = default!;
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest req)
    {
        var doc = await _db.Documents
            .Include(d => d.Process)
            .Include(d => d.WorkflowTrackers.OrderBy(t => t.StepOrder))
            .FirstOrDefaultAsync(d => d.Id == id);

        if (doc == null)
            return NotFound();

        Guid userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var current = doc.WorkflowTrackers.FirstOrDefault(t => t.Id == doc.CurrentStepId);
        if (current == null)
            return BadRequest("У документа нет активного этапа.");

        // --- Отклоняем ---
        current.Status = "Rejected";
        current.RejectedByJson = $"[\"user:{userId}\"]";
        current.CompletedAtUtc = DateTime.UtcNow;
        current.ApproverComment = req.Comment;

        // --- Сам документ тоже закрывается ---
        doc.Status = "Rejected";
        doc.CurrentStepId = null;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Документ отклонён" });
    }
}

public class CreateDocumentRequest
{
    public Guid ProcessId { get; set; }
    public string Title { get; set; } = default!;
    public string FieldsJson { get; set; } = "{}";
    public bool Submit { get; set; } // true = отправить, false = сохранить draft
}