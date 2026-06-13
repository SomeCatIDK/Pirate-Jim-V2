using Discord;
using Discord.WebSocket;
using SomeCatIDK.PirateJim.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SomeCatIDK.PirateJim.Services;

[Service]
public class BlockScamMessageService : IInitializableService
{
    private class SuspiciousData
    {
        public readonly SemaphoreSlim Lock = new(1, 1);
        public List<SocketMessage> SuspiciousMessages = new();

        public int GetUniqueChannels() => SuspiciousMessages.DistinctBy(m => m.Channel.Id).Count();
    }

    private const int MinSuspiciousImages = 3;
    private const int MinChannels = 2;
    private readonly TimeSpan DetectionDuration = TimeSpan.FromMinutes(1);

    private readonly TimeSpan TimeoutDuration = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<ulong, SuspiciousData> _lastSuspiciousMessage = new();

    public BlockScamMessageService(PirateJim bot)
    {
        bot.DiscordClient.MessageReceived += OnScamMessage;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    private async Task OnScamMessage(SocketMessage message)
    {
        if (message.Source != MessageSource.User) return;
        if (message.Author is not SocketGuildUser author) return;

        // Ignore Moderators, should probably include other staff (Supporters) as well
        // But currently no easy way to do that
        if (author.GuildPermissions.ManageMessages) return;

        if (!IsSuspiciousMessage(message)) return;

        SuspiciousData suspiciousData = _lastSuspiciousMessage.GetOrAdd(author.Id, id => new SuspiciousData());
        await suspiciousData.Lock.WaitAsync();

        try
        {
            if (author.IsTimedOut())
            {
                await message.DeleteAsync();
                return;
            }

            suspiciousData.SuspiciousMessages.Add(message);

            // At least two Suspicious Messages required to avoid false positives
            if (suspiciousData.SuspiciousMessages.Count < 2) return;

            // Yes, old Suspicious Messages will pass this. But it is extremely unlikely and wouldn't really matter
            // Fixing it would be removing all outdated suspicious messages each time a new Suspicious Message gets added
            if (suspiciousData.GetUniqueChannels() < MinChannels) return;

            DateTimeOffset minCreatedAt = message.CreatedAt - DetectionDuration;
            if (suspiciousData.SuspiciousMessages[^2].CreatedAt < minCreatedAt) return;

            await author.SetTimeOutAsync(TimeoutDuration);
            for (int i = suspiciousData.SuspiciousMessages.Count - 1; i >= 0; i--)
            {
                var msg = suspiciousData.SuspiciousMessages[i];
                if (msg.CreatedAt < minCreatedAt) break;
                try // Might throw if the message has already been deleted
                {
                    await msg.DeleteAsync();
                }
                catch (Discord.Net.HttpException) { }
            }
            suspiciousData.SuspiciousMessages.Clear();
        }
        finally
        {
            suspiciousData.Lock.Release();
        }
    }

    private static bool IsSuspiciousMessage(SocketMessage message)
    {
        // This needs to be improved if the bots decide to start splitting their messages into smaller chunks to avoid detection
        // In that case keep track of all messages with images as a rolling counter instead of per message, with respect to DetectionDuration
        return message.Attachments.Count(a => a.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase)) >= MinSuspiciousImages;
    }
}
