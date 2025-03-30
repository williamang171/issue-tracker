using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;
using IssueStatsService.Helpers;

namespace IssueStatsService.Consumers;

public class IssueCreatedConsumer(IConnectionMultiplexer muxer) : IConsumer<IssueCreated>
{
    public async Task Consume(ConsumeContext<IssueCreated> context)
    {
        Console.WriteLine("--> Consuming Issue Created: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        // Extract data
        var message = context.Message;
        var issueId = message.Id;
        var projectId = message.ProjectId;
        var status = message.Status;
        var type = message.Type;
        var priority = message.Priority;
        var version = message.Version;

        // Redis keys
        var projectKey = Constants.GetProjectKey(projectId);
        var projectIssueKey = Constants.GetProjectIssueVersionKey(projectId);
        var projectStatusCountsKey = Constants.GetProjectIssueStatusCountsKey(projectId);
        var projectPriorityCountsKey = Constants.GetProjectIssuePriorityCountsKey(projectId);
        var projectTypeCountsKey = Constants.GetProjectIssueTypeCountsKey(projectId);

        // Setup transaction
        IDatabase db = muxer.GetDatabase();
        ITransaction transaction = db.CreateTransaction();
        _ = transaction.AddCondition(Condition.KeyExists(projectKey));
        _ = transaction.HashSetAsync(projectIssueKey, issueId.ToString(), version.ToString());
        _ = transaction.HashIncrementAsync(projectStatusCountsKey, status.ToString());
        _ = transaction.HashIncrementAsync(projectPriorityCountsKey, priority.ToString());
        _ = transaction.HashIncrementAsync(projectTypeCountsKey, type.ToString());
        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new MessageException(typeof(IssueCreated), "--> Failed to commit transaction when consuming Issue Created: " + context.MessageId);
        }

        Console.WriteLine("--> Consumed Issue Created: " + context.MessageId);
    }
}
