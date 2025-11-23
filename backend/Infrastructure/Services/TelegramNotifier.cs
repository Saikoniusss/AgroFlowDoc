using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services;

public interface ITelegramNotifier
{
    ValueTask  SendMessage(long chatId, string message);
    Task SendToUser(Guid userId, string message);
    Task SendToUsers(IEnumerable<Guid> userIds, string message);
    Task SendToRole(string roleName, string message);
}

public class TelegramNotifier : ITelegramNotifier
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBackgroundTaskQueue _queue;

    public TelegramNotifier(IServiceScopeFactory scopeFactory, IBackgroundTaskQueue queue)
    {
        _scopeFactory = scopeFactory;
        _queue = queue;
    }

    public ValueTask SendMessage(long chatId, string message)
    {
        return _queue.QueueBackgroundWorkItemAsync(async ct =>
        {
            using var scope = _scopeFactory.CreateScope();
            var worker = scope.ServiceProvider.GetRequiredService<TelegramNotificationWorker>();

            await worker.SafeSendMessageAsync(chatId, message);
        });
    }
    public async Task SendToUser(Guid userId, string message)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DocflowDbContext>();

        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user?.TelegramChatId is not null)
            await SendMessage(user.TelegramChatId.Value, message);
    }

    public async Task SendToUsers(IEnumerable<Guid> userIds, string message)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DocflowDbContext>();

        var users = await db.Users
            .Where(x => userIds.Contains(x.Id) && x.TelegramChatId != null)
            .ToListAsync();

        foreach (var u in users)
            await SendMessage(u.TelegramChatId!.Value, message);
    }

    public async Task SendToRole(string roleName, string message)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DocflowDbContext>();

        var users = await db.Users
            .Where(x => x.UserRoles.Any(ur => ur.Role.Name == roleName)
                        && x.TelegramChatId != null)
            .ToListAsync();

        foreach (var u in users)
            await SendMessage(u.TelegramChatId!.Value, message);
    }
}