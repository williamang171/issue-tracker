using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;
using IssueStatsService.Helpers;

namespace IssueStatsService.Consumers;

public class IssueUpdatedConsumer(IConnectionMultiplexer muxer) : IConsumer<IssueUpdated>
{
    public async Task Consume(ConsumeContext<IssueUpdated> context)
    {
        Console.WriteLine("--> Consuming Issue Updated: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        // Extract data
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
        var oldVersion = message.OldVersion;
        var newVersion = message.NewVersion;
        var issueId = message.Id;

        // Redis Keys
        var projectKey = Constants.GetProjectKey(projectId);
        var projectIssueKey = Constants.GetProjectIssueVersionKey(projectId);
        var projectStatusCountsKey = Constants.GetProjectIssueStatusCountsKey(projectId);
        var projectPriorityCountsKey = Constants.GetProjectIssuePriorityCountsKey(projectId);
        var projectTypeCountsKey = Constants.GetProjectIssueTypeCountsKey(projectId);

        IDatabase db = muxer.GetDatabase();

        ITransaction transaction = db.CreateTransaction();
        transaction.AddCondition(Condition.KeyExists(projectKey));
        transaction.AddCondition(Condition.HashExists(projectIssueKey, issueId.ToString()));
        transaction.AddCondition(Condition.HashEqual(projectIssueKey, issueId.ToString(), oldVersion.ToString()));
        if (oldStatus.HasValue && newStatus.HasValue && !oldStatus.Equals(newStatus))
        {
            _ = transaction.HashDecrementAsync(projectStatusCountsKey, oldStatus.ToString());
            _ = transaction.HashIncrementAsync(projectStatusCountsKey, newStatus.ToString());
        }

        if (oldPriority.HasValue && newPriority.HasValue && !oldPriority.Equals(newPriority))
        {
            _ = transaction.HashDecrementAsync(projectPriorityCountsKey, oldPriority.ToString());
            _ = transaction.HashIncrementAsync(projectPriorityCountsKey, newPriority.ToString());
        }

        if (oldType.HasValue && newType.HasValue && !oldType.Equals(newType))
        {
            _ = transaction.HashDecrementAsync(projectTypeCountsKey, oldType.ToString());
            _ = transaction.HashIncrementAsync(projectTypeCountsKey, newType.ToString());
        }
        _ = transaction.HashSetAsync(projectIssueKey, issueId.ToString(), newVersion.ToString());

        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new RedisException("--> Failed to commit transaction when consuming Issue Updated: " + context.MessageId);
        }

        Console.WriteLine("--> Consumed Issue Updated: " + context.MessageId);
    }
}
