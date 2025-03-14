using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;

namespace IssueStatsService.Consumers;

public class IssueDeletedConsumer(IConnectionMultiplexer muxer) : IConsumer<IssueDeleted>
{
    public async Task Consume(ConsumeContext<IssueDeleted> context)
    {
        Console.WriteLine("--> Consuming Issue Deleted: " + context.Message.Id);

        // For Debugging
        // var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        // Console.WriteLine(messageJsonString);

        var message = context.Message;
        var projectId = message.ProjectId;
        var status = message.Status;
        var type = message.Type;
        var priority = message.Priority;

        IDatabase db = muxer.GetDatabase();
        ITransaction transaction = db.CreateTransaction();

        _ = transaction.HashDecrementAsync($"project:{projectId}:status-counts", status.ToString());
        _ = transaction.HashDecrementAsync($"project:{projectId}:priority-counts", priority.ToString());
        _ = transaction.HashDecrementAsync($"project:{projectId}:type-counts", type.ToString());

        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new RedisException("--> Failed to commit transaction when consuming Issue Deleted: " + context.Message.Id);
        }

        Console.WriteLine("--> Consumed Issue Deleted: " + context.Message.Id);
    }
}
