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
        // var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        // Console.WriteLine(messageJsonString);

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

        IDatabase db = muxer.GetDatabase();
        ITransaction transaction = db.CreateTransaction();
        if (oldStatus.HasValue && newStatus.HasValue && !oldStatus.Equals(newStatus))
        {
            _ = transaction.HashDecrementAsync($"project:{projectId}:status-counts", oldStatus.ToString());
            _ = transaction.HashIncrementAsync($"project:{projectId}:status-counts", newStatus.ToString());
        }

        if (oldPriority.HasValue && newPriority.HasValue && !oldPriority.Equals(newPriority))
        {
            _ = transaction.HashDecrementAsync($"project:{projectId}:priority-counts", oldPriority.ToString());
            _ = transaction.HashIncrementAsync($"project:{projectId}:priority-counts", newPriority.ToString());
        }

        if (oldType.HasValue && newType.HasValue && !oldType.Equals(newType))
        {
            _ = transaction.HashDecrementAsync($"project:{projectId}:type-counts", oldType.ToString());
            _ = transaction.HashIncrementAsync($"project:{projectId}:type-counts", newType.ToString());
        }

        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new RedisException("--> Failed to commit transaction when consuming Issue Updated: " + context.Message.Id);
        }

        Console.WriteLine("--> Consumed Issue Updated: " + context.Message.Id);
    }
}
