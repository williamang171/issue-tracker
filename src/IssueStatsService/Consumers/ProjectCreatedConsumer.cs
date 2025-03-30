using System;
using System.Text.Json;
using Contracts;
using IssueStatsService.Helpers;
using MassTransit;
using StackExchange.Redis;

namespace IssueStatsService.Consumers;

public class ProjectCreatedConsumer(IConnectionMultiplexer muxer) : IConsumer<ProjectCreated>
{
    public async Task Consume(ConsumeContext<ProjectCreated> context)
    {
        Console.WriteLine("--> Consuming Project Created: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        // Extract data
        var message = context.Message;
        var projectId = message.Id;

        // Redis Keys
        var key = Constants.GetProjectKey(projectId);
        var projectStatusCountsKey = Constants.GetProjectIssueStatusCountsKey(projectId);
        var projectPriorityCountsKey = Constants.GetProjectIssuePriorityCountsKey(projectId);
        var projectTypeCountsKey = Constants.GetProjectIssueTypeCountsKey(projectId);

        IDatabase db = muxer.GetDatabase();
        ITransaction transaction = db.CreateTransaction();
        _ = transaction.StringSetAsync(key, projectId.ToString());
        _ = transaction.HashSetAsync(projectStatusCountsKey, IssueStatus.Open.ToString(), 0);
        _ = transaction.HashSetAsync(projectStatusCountsKey, IssueStatus.InProgress.ToString(), 0);
        _ = transaction.HashSetAsync(projectStatusCountsKey, IssueStatus.Resolved.ToString(), 0);
        _ = transaction.HashSetAsync(projectStatusCountsKey, IssueStatus.Closed.ToString(), 0);
        _ = transaction.HashSetAsync(projectPriorityCountsKey, IssuePriority.Low.ToString(), 0);
        _ = transaction.HashSetAsync(projectPriorityCountsKey, IssuePriority.Medium.ToString(), 0);
        _ = transaction.HashSetAsync(projectPriorityCountsKey, IssuePriority.High.ToString(), 0);
        _ = transaction.HashSetAsync(projectPriorityCountsKey, IssuePriority.Critical.ToString(), 0);
        _ = transaction.HashSetAsync(projectTypeCountsKey, IssueType.Bug.ToString(), 0);
        _ = transaction.HashSetAsync(projectTypeCountsKey, IssueType.FeatureRequest.ToString(), 0);
        _ = transaction.HashSetAsync(projectTypeCountsKey, IssueType.Other.ToString(), 0);

        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new MessageException(typeof(ProjectCreated), "--> Failed to commit transaction when consuming Project Created: " + context.MessageId);
        }

        Console.WriteLine("--> Consumed Project Created: " + context.MessageId);
    }
}
