using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;

namespace IssueStatsService.Consumers;

public class ProjectDeletedConsumer(IConnectionMultiplexer muxer) : IConsumer<ProjectDeleted>
{
    public async Task Consume(ConsumeContext<ProjectDeleted> context)
    {
        Console.WriteLine("--> Consuming Project Deleted: " + context.Message.Id);
        var message = context.Message;
        var projectId = message.Id;

        IDatabase db = muxer.GetDatabase();
        ITransaction transaction = db.CreateTransaction();
        _ = transaction.KeyExpireAsync($"project:{projectId}:status-counts", TimeSpan.FromHours(1));
        _ = transaction.KeyExpireAsync($"project:{projectId}:priority-counts", TimeSpan.FromHours(1));
        _ = transaction.KeyExpireAsync($"project:{projectId}:type-counts", TimeSpan.FromHours(1));

        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new RedisException("--> Failed to commit transaction when consuming Project Deleted: " + context.Message.Id);
        }

        Console.WriteLine("--> Consumed Project Deleted: " + context.Message.Id);
    }
}
