using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly DocflowDbContext _db;

    public ProfileController(DocflowDbContext db)
    {
        _db = db;
    }

    // Получение данных профиля
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            user.Id,
            user.DisplayName,
            user.Email,
            user.TelegramUsername,
            user.TelegramChatId
        });
    }

    // Обновление профиля
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        var id = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.DisplayName = req.DisplayName;
        user.Email = req.Email;
        user.TelegramUsername = req.TelegramUsername;
        user.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(new { message = "Профиль обновлён" });
    }

    // Приём chat_id от Telegram-бота
    [AllowAnonymous]
    [HttpPost("telegram-link")]
    public async Task<IActionResult> LinkTelegram([FromBody] TelegramLinkRequest req)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.TelegramUsername == req.Username);
        if (user == null)
            return NotFound();

        user.TelegramChatId = req.ChatId;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Telegram-аккаунт привязан" });
    }
}

public record UpdateProfileRequest(string DisplayName, string Email, string? TelegramUsername);
public record TelegramLinkRequest(string Username, long ChatId);