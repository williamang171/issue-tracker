using System;
using System.Text.Json;
using Contracts;
using MassTransit;
using StackExchange.Redis;
using IssueStatsService.DTOs;
using IssueStatsService.Helpers;

namespace IssueStatsService.Consumers;

public class UserUpdatedConsumer(IConnectionMultiplexer muxer) : IConsumer<UserUpdated>
{
    public async Task Consume(ConsumeContext<UserUpdated> context)
    {
        Console.WriteLine("--> Consuming User updated: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        // Extract data
        var message = context.Message;
        var userName = message.UserName;
        var newValues = message.NewValues;
        var oldVersion = message.OldVersion;
        var newVersion = message.NewVersion;

        IDatabase db = muxer.GetDatabase();
        string key = Constants.GetUserKey(userName);

        // If key doesn't exist, throw
        RedisValue storedUser = await db.StringGetAsync(key);
        if (!storedUser.HasValue)
        {
            throw new MessageException(typeof(UserCreated), $"User with {userName} not found");
        }

        // If failed to create userDto from source, throw
        var userDto = JsonSerializer.Deserialize<UserDto>(storedUser!);
        if (userDto == null)
        {
            throw new MessageException(typeof(UserCreated), $"Failed to create userDto for {userName}");
        }

        // If version don't match, process later by throwing
        if (userDto.Version != oldVersion)
        {
            throw new MessageException(typeof(UserCreated), $"Version mismatch for ${userName}, current version {userDto.Version}, received version {oldVersion}");
        }

        // Update user
        userDto.RoleCode = newValues.RoleCode ?? userDto.RoleCode;
        userDto.IsActive = newValues.IsActive ?? userDto.IsActive;
        userDto.Version = newVersion;
        var jsonUser = JsonSerializer.Serialize(userDto);
        await db.StringSetAsync(key, jsonUser);
        Console.WriteLine("--> Consumed User updated: " + context.MessageId);
    }
}
