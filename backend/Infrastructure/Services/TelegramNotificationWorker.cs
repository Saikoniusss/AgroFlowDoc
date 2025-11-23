using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace Infrastructure.Services;

public class TelegramNotificationWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<TelegramNotificationWorker> _logger;
    private readonly TelegramBotClient _bot;

    private const int RETRY_COUNT = 5;
    private const int THROTTLE_DELAY_MS = 60; // 60 ms между отправками (≈ 16 msg/sec)

    public TelegramNotificationWorker(
        IBackgroundTaskQueue queue,
        ILogger<TelegramNotificationWorker> logger,
        IConfiguration config)
    {
        _queue = queue;
        _logger = logger;

        var token = config["Telegram:BotToken"];
        _bot = new TelegramBotClient(token);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 TelegramNotificationWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);

            try
            {
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing Telegram task");
            }

            await Task.Delay(THROTTLE_DELAY_MS, stoppingToken);
        }
    }

    // Метод отправки с retry
    public async Task SafeSendMessageAsync(long chatId, string message)
    {
        for (int attempt = 1; attempt <= RETRY_COUNT; attempt++)
        {
            try
            {
                await _bot.SendTextMessageAsync(
                    chatId,
                    message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html
                );
                return;
            }
            catch (ApiRequestException ex) when (ex.ErrorCode == 429)
            {
                // Telegram flood control
                int delay = ex.Parameters?.RetryAfter ?? 3;
                await Task.Delay(delay * 1000);
            }
            catch (Exception ex)
            {
                if (attempt == RETRY_COUNT)
                    throw;

                await Task.Delay(500 * attempt);
            }
        }
    }
}
