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
    private readonly IConfiguration _config;

    public ProfileController(DocflowDbContext db, IConfiguration configuration)
    {
        _db = db;
        _config = configuration;
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
            user.TelegramChatId,
            user.AvatarPath
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
        user.AvatarPath = req.AvatarPath;

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
    [HttpPost("upload-avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile photo)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var user = await _db.Users.FindAsync(userId);
        if (user == null)
            return Unauthorized();

        if (photo == null || photo.Length == 0)
            return BadRequest("Файл пустой");
        
        var basePath = _config.GetValue<string>("FileStorage:BasePath")
                     ?? Path.Combine(Directory.GetCurrentDirectory(), "DocumentUploads");
        var avatarFolder = Path.Combine(basePath, "avatars");
        Directory.CreateDirectory(avatarFolder);

        // Генерируем имя
        var ext = Path.GetExtension(photo.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(avatarFolder, fileName);

        using (var stream = System.IO.File.Create(filePath))
        {
            await photo.CopyToAsync(stream);
        }

        // Сохраняем только относительный путь
        user.AvatarPath = $"avatars/{fileName}";
        await _db.SaveChangesAsync();

        return Ok(new { photo = user.AvatarPath });
    }
}

public record UpdateProfileRequest(string DisplayName, string Email, string? TelegramUsername, string AvatarPath);
public record TelegramLinkRequest(string Username, long ChatId);