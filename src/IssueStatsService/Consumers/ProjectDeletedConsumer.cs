using System;
using System.Text.Json;
using Contracts;
using IssueStatsService.Helpers;
using MassTransit;
using StackExchange.Redis;

namespace IssueStatsService.Consumers;

public class ProjectDeletedConsumer(IConnectionMultiplexer muxer) : IConsumer<ProjectDeleted>
{
    public async Task Consume(ConsumeContext<ProjectDeleted> context)
    {
        Console.WriteLine("--> Consuming Project Deleted: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        // Extract data
        var message = context.Message;
        var projectId = message.Id;

        // Redis Keys
        var projectKey = Constants.GetProjectKey(projectId);
        var projectIssueVersionKey = Constants.GetProjectIssueVersionKey(projectId);
        var projectStatusCountsKey = Constants.GetProjectIssueStatusCountsKey(projectId);
        var projectPriorityCountsKey = Constants.GetProjectIssuePriorityCountsKey(projectId);
        var projectTypeCountsKey = Constants.GetProjectIssueTypeCountsKey(projectId);
        var projectAssignmentsKey = Constants.GetProjectAssignmenstKey(projectId);

        IDatabase db = muxer.GetDatabase();
        ITransaction transaction = db.CreateTransaction();
        _ = transaction.AddCondition(Condition.KeyExists(projectKey));
        _ = transaction.KeyDeleteAsync(projectStatusCountsKey);
        _ = transaction.KeyDeleteAsync(projectPriorityCountsKey);
        _ = transaction.KeyDeleteAsync(projectTypeCountsKey);
        _ = transaction.KeyDeleteAsync(projectKey);
        _ = transaction.KeyDeleteAsync(projectIssueVersionKey);
        _ = transaction.KeyDeleteAsync(projectAssignmentsKey);

        bool committed = await transaction.ExecuteAsync();

        if (!committed)
        {
            throw new MessageException(typeof(ProjectDeleted), "--> Failed to commit transaction when consuming Project Deleted: " + context.MessageId);
        }

        Console.WriteLine("--> Consumed Project Deleted: " + context.MessageId);
    }
}
