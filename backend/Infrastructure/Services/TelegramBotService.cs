using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public class TelegramBotService : BackgroundService
{
    private readonly ILogger<TelegramBotService> _logger;
    private readonly IConfiguration _config;
    private readonly IServiceScopeFactory _scopeFactory;
    private TelegramBotClient? _bot;

    public TelegramBotService(ILogger<TelegramBotService> logger, IConfiguration config, 
            IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _config = config;
        _scopeFactory = scopeFactory;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = _config["Telegram:BotToken"];
        if (string.IsNullOrEmpty(token))
        {
            _logger.LogWarning("❗ Telegram bot token not configured");
            return;
        }

        _bot = new TelegramBotClient(token);


        await _bot.DeleteWebhookAsync(true, stoppingToken);
        var me = await _bot.GetMeAsync(stoppingToken);
        _logger.LogInformation("🤖 Telegram bot {Name} started.", me.Username);

        var options = new Telegram.Bot.Polling.ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>()
        };

        _bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, options, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        if (update.Type != UpdateType.Message || update.Message?.From == null)
            return;

        var msg = update.Message;
        var username = msg.From.Username;
        var chatId = msg.Chat.Id;

        if (msg.Text is not null && msg.Text.StartsWith("/start"))
        {
            if (string.IsNullOrEmpty(username))
            {
                await bot.SendTextMessageAsync(chatId,
                    "❗ У тебя нет username. Укажи его в настройках Telegram и повтори команду /start.",
                    cancellationToken: ct);
                return;
            }

            // Создаём scope для DbContext
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DocflowDbContext>();

            var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramUsername == username, ct);
            if (user is null)
            {
                await bot.SendTextMessageAsync(chatId,
                    "⚠️ Твой Telegram username не найден в системе. Укажи его в профиле AgroFlow и повтори /start.",
                    cancellationToken: ct);
                return;
            }

            user.TelegramChatId = chatId;
            await db.SaveChangesAsync(ct);

            await bot.SendTextMessageAsync(chatId,
                $"✅ Привет, {user.DisplayName}! Твой Telegram успешно привязан к AgroFlow.",
                cancellationToken: ct);
            _logger.LogInformation("User {User} linked Telegram chat ID {ChatId}", username, chatId);
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        _logger.LogError(ex, "Telegram bot error");
        return Task.CompletedTask;
    }
}