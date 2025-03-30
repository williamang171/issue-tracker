using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;
using IssueStatsService.Helpers;

namespace IssueStatsService.Consumers;

public class IssueDeletedConsumer(IConnectionMultiplexer muxer) : IConsumer<IssueDeleted>
{
    public async Task Consume(ConsumeContext<IssueDeleted> context)
    {
        Console.WriteLine("--> Consuming Issue Deleted: " + context.MessageId);

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

        // Redis Keys
        var projectKey = Constants.GetProjectKey(projectId);
        var projectIssueKey = Constants.GetProjectIssueVersionKey(projectId);
        var projectStatusCountsKey = Constants.GetProjectIssueStatusCountsKey(projectId);
        var projectPriorityCountsKey = Constants.GetProjectIssuePriorityCountsKey(projectId);
        var projectTypeCountsKey = Constants.GetProjectIssueTypeCountsKey(projectId);

        // Setup Transaction
        IDatabase db = muxer.GetDatabase();
        ITransaction transaction = db.CreateTransaction();
        _ = transaction.AddCondition(Condition.KeyExists(projectKey));
        _ = transaction.AddCondition(Condition.HashExists(projectIssueKey, issueId.ToString()));
        _ = transaction.AddCondition(Condition.HashEqual(projectIssueKey, issueId.ToString(), version.ToString()));
        _ = transaction.HashDecrementAsync(projectStatusCountsKey, status.ToString());
        _ = transaction.HashDecrementAsync(projectPriorityCountsKey, priority.ToString());
        _ = transaction.HashDecrementAsync(projectTypeCountsKey, type.ToString());
        _ = transaction.HashDeleteAsync(projectIssueKey, issueId.ToString());

        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new MessageException(typeof(IssueDeleted), "--> Failed to commit transaction when consuming Issue Deleted: " + context.MessageId);
        }

        Console.WriteLine("--> Consumed Issue Deleted: " + context.MessageId);
    }
}
