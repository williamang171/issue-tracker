using System;
using System.Text.Json;
using AutoMapper;
using Contracts;
using MassTransit;
using ProjectIssueService.Data;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;

namespace ProjectIssueService.Consumers;

public class UserCreatedConsumer(ApplicationDbContext dbContext, IMapper mapper) : IConsumer<UserCreated>
{
    public async Task Consume(ConsumeContext<UserCreated> context)
    {
        Console.WriteLine("--> Consuming User Created: " + context.Message.Id);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        var message = context.Message;
        var userName = message.UserName;
        var lastLoginTime = message.LastLoginTime;

        UserDto userDto = new()
        {
            UserName = userName,
            LastLoginTime = lastLoginTime,
        };
        var user = mapper.Map<User>(userDto);
        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        Console.WriteLine("--> Consumed User Created: " + context.Message.Id);
    }
}
