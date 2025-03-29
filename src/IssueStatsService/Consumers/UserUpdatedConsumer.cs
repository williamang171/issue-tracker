using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;
using IssueStatsService.DTOs;

namespace IssueStatsService.Consumers;

public class UserUpdatedConsumer(IConnectionMultiplexer muxer) : IConsumer<UserUpdated>
{
    public async Task Consume(ConsumeContext<UserUpdated> context)
    {
        Console.WriteLine("--> Consuming User updated: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        var message = context.Message;
        var userName = message.UserName;
        var newValues = message.NewValues;

        IDatabase db = muxer.GetDatabase();
        string key = $"user:{userName}";

        // If key doesn't exist, throw
        RedisValue storedUser = await db.StringGetAsync(key);
        if (!storedUser.HasValue)
        {
            throw new MessageException(typeof(UserCreated), $"User with ${userName} not found");
        }

        // If failed to create userDto from source, throw
        var userDto = JsonSerializer.Deserialize<UserDto>(storedUser!);
        if (userDto == null)
        {
            throw new MessageException(typeof(UserCreated), $"Failed to create userDto for ${userName}");
        }

        // Update user
        userDto.RoleCode = newValues.RoleCode ?? userDto.RoleCode;
        userDto.IsActive = newValues.IsActive ?? userDto.IsActive;
        var jsonUser = JsonSerializer.Serialize(userDto);
        await db.StringSetAsync(key, jsonUser);

        Console.WriteLine("--> Consumed User updated: " + context.MessageId);
    }
}
