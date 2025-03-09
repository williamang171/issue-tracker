using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;

namespace IssueStatsService.Consumers;

public class IssueUpdatedConsumer(IConnectionMultiplexer muxer) : IConsumer<IssueUpdated>
{
    public async Task Consume(ConsumeContext<IssueUpdated> context)
    {
        Console.WriteLine("--> Consuming Issue Updated: " + context.Message.Id);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        IDatabase db = muxer.GetDatabase();
        var message = context.Message;
        var oldValues = message.OldValues;
        var newValues = message.NewValues;
        var projectId = message.ProjectId;
        var oldStatus = oldValues.Status;
        var oldType = oldValues.Type;
        var oldPriority = oldValues.Priority;
        var newStatus = newValues.Status;
        var newType = newValues.Type;
        var newPriority = newValues.Priority;

        if (oldStatus.HasValue && newStatus.HasValue && !oldStatus.Equals(newStatus))
        {
            await db.HashDecrementAsync($"project:{projectId}:status-counts", oldStatus.ToString());
            await db.HashIncrementAsync($"project:{projectId}:status-counts", newStatus.ToString());
        }

        if (oldPriority.HasValue && newPriority.HasValue && !oldPriority.Equals(newPriority))
        {
            await db.HashDecrementAsync($"project:{projectId}:priority-counts", oldPriority.ToString());
            await db.HashIncrementAsync($"project:{projectId}:priority-counts", newPriority.ToString());
        }

        if (oldType.HasValue && newType.HasValue && !oldType.Equals(newType))
        {
            await db.HashDecrementAsync($"project:{projectId}:type-counts", oldType.ToString());
            await db.HashIncrementAsync($"project:{projectId}:type-counts", newType.ToString());
        }

        Console.WriteLine("--> Consumed Issue Updated: " + context.Message.Id);
    }
}
