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
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        IDatabase db = muxer.GetDatabase();
        var message = context.Message;
        var projectId = message.ProjectId;
        var status = message.Status;
        var type = message.Type;
        var priority = message.Priority;

        await db.HashIncrementAsync($"project:{projectId}:status-counts", status.ToString());
        await db.HashIncrementAsync($"project:{projectId}:priority-counts", priority.ToString());
        await db.HashIncrementAsync($"project:{projectId}:type-counts", type.ToString());

        Console.WriteLine("--> Consumed Issue Created: " + context.Message.Id);
    }
}
