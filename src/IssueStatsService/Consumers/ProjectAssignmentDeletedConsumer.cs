using System;
using System.Text.Json;
using Contracts;
using IssueStatsService.Helpers;
using MassTransit;
using StackExchange.Redis;

namespace IssueStatsService.Consumers;


public class ProjectAssignmentDeletedConsumer(IConnectionMultiplexer muxer) : IConsumer<ProjectAssignmentDeleted>
{
    public async Task Consume(ConsumeContext<ProjectAssignmentDeleted> context)
    {
        Console.WriteLine("--> Consuming Project Assignment Deleted: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        var message = context.Message;
        var projectId = message.ProjectId;
        var userName = message.UserName;

        IDatabase db = muxer.GetDatabase();

        string setKey = Constants.GetProjectAssignmenstKey(projectId);

        // Check if user already exists in set
        var contains = await db.SetContainsAsync(setKey, userName);
        if (contains)
        {
            await db.SetRemoveAsync(setKey, userName);
        }
        else
        {
            throw new MessageException(typeof(ProjectAssignmentDeleted), $"User ${userName} does not exist in project ${projectId}");
        }

        Console.WriteLine("--> Consumed Project Assignment Deleted: " + context.MessageId);
    }
}
