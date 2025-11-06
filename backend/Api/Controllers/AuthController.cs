using Application.Auth.Contracts;
using Common.Security;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DocflowDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(DocflowDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    // Регистрация
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (await _db.Users.AnyAsync(u => u.Username == req.Username))
            return Conflict(new { message = "Пользователь с таким логином уже существует" });

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = req.Username,
            PasswordHash = PasswordHasher.Hash(req.Password),
            DisplayName = req.DisplayName,
            Email = req.Email,
            IsApproved = false,
            IsActive = true
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return Ok(new { message = "Регистрация выполнена. Ожидайте подтверждения администратора." });
    }

    // Логин
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == req.Username);

        if (user is null || !PasswordHasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "Неверный логин или пароль" });

        if (!user.IsActive)
            return Forbid();

        if (!user.IsApproved)
            return StatusCode(403, new { message = "Учётная запись ожидает подтверждения администратором" });

        // Создание JWT токена
        var jwtSettings = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.DisplayName)
        };

        foreach (var role in user.UserRoles.Select(r => r.Role.Name))
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpireMinutes"]!)),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            access_token = tokenString,
            user = new
            {
                user.Id,
                user.DisplayName,
                user.Username,
                Roles = user.UserRoles.Select(r => r.Role.Name)
            }
        });
    }

    // Текущий пользователь
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id is null)
            return Unauthorized();

        var user = await _db.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == Guid.Parse(id));

        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.DisplayName,
            user.Email,
            user.IsApproved,
            Roles = user.UserRoles.Select(r => r.Role.Name)
        });
    }
}