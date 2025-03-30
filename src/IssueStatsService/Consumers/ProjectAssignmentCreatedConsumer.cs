using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;
using IssueStatsService.Helpers;

namespace IssueStatsService.Consumers;


public class ProjectAssignmentCreatedConsumer(IConnectionMultiplexer muxer) : IConsumer<ProjectAssignmentCreated>
{
    public async Task Consume(ConsumeContext<ProjectAssignmentCreated> context)
    {
        Console.WriteLine("--> Consuming Project Assignment Created: " + context.MessageId);

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
        if (!contains)
        {
            await db.SetAddAsync(setKey, userName);
        }
        else
        {
            throw new MessageException(typeof(ProjectAssignmentCreated), $"User ${userName} already exists in project ${projectId}");
        }

        Console.WriteLine("--> Consumed Project Assignment Created: " + context.MessageId);
    }
}
