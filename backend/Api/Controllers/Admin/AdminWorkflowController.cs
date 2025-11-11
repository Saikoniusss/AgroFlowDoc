using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.Worfkflow.Contracts;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/workflow")]
[Authorize(Roles = "Administrator")]
public class AdminWorkflowController : ControllerBase
{
    private readonly DocflowDbContext _db;
    public AdminWorkflowController(DocflowDbContext db) => _db = db;

    // ---------- TEMPLATES ----------

    [HttpGet("templates")]
    public async Task<IActionResult> GetTemplates()
    {
        var list = await _db.DocumentTemplates
            .Include(t => t.Fields.OrderBy(f => f.Order))
            .ToListAsync();

        return Ok(list);
    }

    [HttpPost("templates")]
    public async Task<IActionResult> CreateTemplate([FromBody] CreateDocumentTemplateRequest  req)
    {
        if (await _db.DocumentTemplates.AnyAsync(t => t.Code == req.Code))
            return Conflict(new { message = "Template code already exists" });
        var entity = new DocumentTemplate {
                Id = Guid.NewGuid(),
                Name = req.Name,
                Code = req.Code,
                Description = req.Description
            };
        _db.DocumentTemplates.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetTemplates), new { id = entity.Id }, entity);
    }

    [HttpPost("templates/{id}/fields")]
    public async Task<IActionResult> AddField(Guid id, [FromBody] CreateTemplateFieldRequest req)
    {
        var template = await _db.DocumentTemplates.FindAsync(id);
        if (template == null) return NotFound();

        var field = new DocumentTemplateField
        {
            Id = Guid.NewGuid(),
            TemplateId = id,
            Name = req.Name,
            Label = req.Label,
            FieldType = req.FieldType,
            IsRequired = req.IsRequired,
            Order = req.Order,
            OptionsJson = req.OptionsJson
        };

        _db.DocumentTemplateFields.Add(field);
        await _db.SaveChangesAsync();
        return Ok(field);
    }
    [HttpPut("templates/{id:guid}")]
    public async Task<IActionResult> UpdateTemplate(Guid id, [FromBody] UpdateTemplateRequest req)
    {
        var template = await _db.DocumentTemplates.FindAsync(id);
        if (template == null)
            return NotFound();

        template.Name = req.Name;
        template.Code = req.Code;
        template.Description = req.Description;
        await _db.SaveChangesAsync();

        return Ok(template);
    }

    [HttpPut("templates/{templateId:guid}/fields/{fieldId:guid}")]
    public async Task<IActionResult> UpdateField(Guid templateId, Guid fieldId, [FromBody] UpdateFieldRequest req)
    {
        var field = await _db.DocumentTemplateFields
            .FirstOrDefaultAsync(f => f.Id == fieldId && f.TemplateId == templateId);
        if (field == null)
            return NotFound();

        field.Name = req.Name;
        field.Label = req.Label;
        field.FieldType = req.FieldType;
        field.IsRequired = req.IsRequired;
        field.Order = req.Order;
        field.OptionsJson = req.OptionsJson;

        await _db.SaveChangesAsync();
        return Ok(field);
    }

    [HttpDelete("templates/{templateId:guid}/fields/{fieldId:guid}")]
    public async Task<IActionResult> DeleteField(Guid templateId, Guid fieldId)
    {
        var field = await _db.DocumentTemplateFields
            .FirstOrDefaultAsync(f => f.Id == fieldId && f.TemplateId == templateId);
        if (field == null)
            return NotFound();

        _db.DocumentTemplateFields.Remove(field);
        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPut("templates/{id}/fields/reorder")]
    public async Task<IActionResult> ReorderFields(Guid id, [FromBody] List<DocumentTemplateField> fields)
    {
        var template = await _db.DocumentTemplates
            .Include(t => t.Fields)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (template == null) return NotFound();

        foreach (var field in fields)
        {
            var dbField = template.Fields.FirstOrDefault(f => f.Id == field.Id);
            if (dbField != null)
                dbField.Order = field.Order;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ---------- ROUTES ----------

    [HttpGet("routes")]
    public async Task<IActionResult> GetRoutes()
    {
        var routes = await _db.WorkflowRoutes
            .Include(r => r.Steps)
            .Select(r => new {
                r.Id,
                r.Name,
                r.Code,
                r.Description,
                Steps = r.Steps.OrderBy(s => s.StepOrder).Select(s => new {
                    s.Id,
                    s.StepName,
                    s.StepOrder,
                    s.IsParallel,
                    s.MinApprovals,
                    s.ApproversJson
                })
            }).ToListAsync();

        return Ok(routes);
    }

    [HttpPost("routes")]
    public async Task<IActionResult> CreateRoute([FromBody] CreateWorkflowRouteRequest  req)
    {
        if (await _db.WorkflowRoutes.AnyAsync(r => r.Code == req.Code))
            return Conflict(new { message = "Route code already exists" });

        var route = new WorkflowRoute {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Code = req.Code,
            Description = req.Description
        };

        _db.WorkflowRoutes.Add(route);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetRoutes), new { id = route.Id }, route);
    }

    [HttpPost("routes/{id}/steps")]
    public async Task<IActionResult> AddStep(Guid id, [FromBody] CreateWorkflowStepRequest req)
    {
        if (!await _db.WorkflowRoutes.AnyAsync(r => r.Id == id))
            return NotFound(new { message = "Route not found" });

        var step = new WorkflowRouteStep
        {
            Id = Guid.NewGuid(),
            RouteId = id,
            StepOrder = req.StepOrder,
            StepName = req.StepName,
            IsParallel = req.IsParallel,
            MinApprovals = req.MinApprovals,
            ApproversJson = req.ApproversJson
        };

        _db.WorkflowRouteSteps.Add(step);
        await _db.SaveChangesAsync();
        return Ok(step);
    }
    
    public class ReorderStepDto
    {
        public Guid Id { get; set; }
        public int StepOrder { get; set; }
    }

    [HttpPut("routes/{id}/steps/reorder")]
    public async Task<IActionResult> ReorderSteps(Guid id, [FromBody] List<ReorderStepDto> steps)
    {
        var route = await _db.WorkflowRoutes
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (route == null)
            return NotFound();

        foreach (var s in steps)
        {
            var dbStep = route.Steps.FirstOrDefault(x => x.Id == s.Id);
            if (dbStep != null)
                dbStep.StepOrder = s.StepOrder;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ---------- PROCESSES ----------

    [HttpGet("processes")]
    public async Task<IActionResult> GetProcesses()
    {
        var processes = await _db.Processes
            .Include(p => p.DocumentTemplate)
            .Include(p => p.WorkflowRoute)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Code, p.Description,
                Template = new { p.DocumentTemplate.Id, p.DocumentTemplate.Name },
                Route = new { p.WorkflowRoute.Id, p.WorkflowRoute.Name }
            })
            .ToListAsync();

        return Ok(processes);
    }

    [HttpPost("processes")]
    public async Task<IActionResult> CreateProcess([FromBody] CreateProcessRequest req)
    {
        if (!await _db.DocumentTemplates.AnyAsync(t => t.Id == req.DocumentTemplateId))
            return BadRequest(new { message = "Invalid DocumentTemplateId" });

        if (!await _db.WorkflowRoutes.AnyAsync(r => r.Id == req.WorkflowRouteId))
            return BadRequest(new { message = "Invalid WorkflowRouteId" });

        if (await _db.Processes.AnyAsync(p => p.Code == req.Code))
            return Conflict(new { message = "Process code already exists" });

        var entity = new Process
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Code = req.Code,
            Description = req.Description,
            DocumentTemplateId = req.DocumentTemplateId,
            WorkflowRouteId = req.WorkflowRouteId
        };

        _db.Processes.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProcesses), new { id = entity.Id }, entity);
    }
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Where(u => u.IsActive && u.IsApproved)
            .Select(u => new { u.Id, u.DisplayName, u.Username })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.Roles
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        return Ok(roles);
    }
    [HttpPut("routes/{routeId:guid}/steps/{stepId:guid}")]
    public async Task<IActionResult> UpdateStep(Guid routeId, Guid stepId, [FromBody] CreateWorkflowStepRequest req)
    {
        var step = await _db.WorkflowRouteSteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.RouteId == routeId);

        if (step == null)
            return NotFound(new { message = "Step not found" });

        step.StepName = req.StepName;
        step.StepOrder = req.StepOrder;
        step.IsParallel = req.IsParallel;
        step.MinApprovals = req.MinApprovals;
        step.ApproversJson = req.ApproversJson ?? "[]";

        await _db.SaveChangesAsync();
        return Ok(step);
    }
}