using Application.Auth.Contracts;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class AdminController : ControllerBase
{
    private readonly DocflowDbContext _db;

    public AdminController(DocflowDbContext db)
    {
        _db = db;
    }

    //Получить список всех пользователей
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.DisplayName,
                u.Email,
                u.IsActive,
                u.IsApproved,
                Roles = u.UserRoles.Select(r => r.Role.Name).ToList()
            })
            .ToListAsync();

        return Ok(users);
    }

    //Подтверждение пользователя
    [HttpPost("approve/{id:guid}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return NotFound();
        user.IsApproved = true;
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Пользователь {user.Username} подтвержден" });
    }

    // ✅ Назначить роль пользователю
    [HttpPost("assign-role")]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest req)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Id == req.UserId);

        if (user is null)
            return NotFound("Пользователь не найден");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == req.RoleName);
        if (role is null)
            return NotFound("Роль не найдена");

        if (user.UserRoles.Any(r => r.RoleId == role.Id))
            return BadRequest("Пользователь уже имеет эту роль");

        user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Роль '{role.Name}' назначена пользователю {user.Username}" });
    }

    // ✅ Деактивировать пользователя
    [HttpPost("deactivate/{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user is null)
            return NotFound();

        user.IsActive = false;
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Пользователь {user.Username} деактивирован" });
    }

    // ✅ Получить список всех ролей
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _db.Roles
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Description,
                UsersCount = _db.UserRoles.Count(ur => ur.RoleId == r.Id)
            })
            .ToListAsync();

        return Ok(roles);
    }

    // ✅ Создать роль
    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest req)
    {
        if (await _db.Roles.AnyAsync(r => r.Name == req.Name))
            return Conflict("Роль с таким именем уже существует");

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Description = req.Description
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return Ok(role);
    }

    // ✅ Обновить роль
    [HttpPut("roles/{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] CreateRoleRequest req)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role is null)
            return NotFound();

        role.Name = req.Name;
        role.Description = req.Description;
        await _db.SaveChangesAsync();
        return Ok(role);
    }

    // ✅ Удалить роль
    [HttpDelete("roles/{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role is null)
            return NotFound();

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
        return Ok(new { message = $"Роль '{role.Name}' удалена" });
    }
    [HttpPost("deactivate-role")]
public async Task<IActionResult> RemoveRole([FromBody] AssignRoleRequest req)
{
    var userRole = await _db.UserRoles
        .Include(ur => ur.Role)
        .FirstOrDefaultAsync(ur => ur.UserId == req.UserId && ur.Role.Name == req.RoleName);

    if (userRole is null)
        return NotFound("Связь не найдена");

    _db.UserRoles.Remove(userRole);
    await _db.SaveChangesAsync();
    return Ok(new { message = "Роль снята с пользователя" });
}

}