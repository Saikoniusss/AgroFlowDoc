using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UsersController : ControllerBase
{
    private readonly DocflowDbContext _db;
    public UsersController(DocflowDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.CreatedAtUtc)
            .ToListAsync();
        return Ok(users);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null) return NotFound();
        user.IsApproved = true;
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Пользователь {user.DisplayName} подтверждён." });
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRoles(Guid id, [FromBody] List<Guid> roleIds)
    {
        var user = await _db.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        user.UserRoles.Clear();
        foreach (var rid in roleIds)
            user.UserRoles.Add(new UserRole { UserId = id, RoleId = rid });

        await _db.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] Role role)
    {
        role.Id = Guid.NewGuid();
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return Ok(role);
    }

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles() => Ok(await _db.Roles.ToListAsync());
}