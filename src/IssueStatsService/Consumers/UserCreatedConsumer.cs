using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;
using IssueStatsService.DTOs;

namespace IssueStatsService.Consumers;

public class UserCreatedConsumer(IConnectionMultiplexer muxer) : IConsumer<UserCreated>
{
    public async Task Consume(ConsumeContext<UserCreated> context)
    {
        Console.WriteLine("--> Consuming User Created: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        var message = context.Message;
        var userName = message.UserName;
        var roleCode = message.RoleCode;
        var isActive = message.IsActive;
        var user = new UserDto()
        {
            UserName = userName,
            RoleCode = roleCode,
            IsActive = isActive
        };
        var jsonUser = JsonSerializer.Serialize(user);

        IDatabase db = muxer.GetDatabase();
        string key = $"user:{userName}";

        // Check if user already exists in set
        var val = await db.StringGetAsync(key);
        if (val.IsNull)
        {
            await db.StringSetAsync(key, jsonUser);
        }
        else
        {
            throw new MessageException(typeof(UserCreated), $"User ${userName} already exists");
        }

        Console.WriteLine("--> Consumed User Created: " + context.MessageId);
    }
}
