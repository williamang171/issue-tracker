using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;

namespace IssueStatsService.Consumers;

public class IssueCreatedConsumer(IConnectionMultiplexer muxer) : IConsumer<IssueCreated>
{
    public async Task Consume(ConsumeContext<IssueCreated> context)
    {
        Console.WriteLine("--> Consuming Issue Created: " + context.Message.Id);

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
        _ = transaction.HashIncrementAsync($"project:{projectId}:status-counts", status.ToString());
        _ = transaction.HashIncrementAsync($"project:{projectId}:priority-counts", priority.ToString());
        _ = transaction.HashIncrementAsync($"project:{projectId}:type-counts", type.ToString());
        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new RedisException("--> Failed to commit transaction when consuming Issue Created: " + context.Message.Id);
        }

        Console.WriteLine("--> Consumed Issue Created: " + context.Message.Id);
    }
}
