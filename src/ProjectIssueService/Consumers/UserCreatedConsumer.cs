using System;
using System.Text.Json;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Data;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;

namespace ProjectIssueService.Consumers;

public class UserCreatedConsumer(ApplicationDbContext dbContext, IMapper mapper) : IConsumer<UserCreated>
{
    public async Task Consume(ConsumeContext<UserCreated> context)
    {
        Console.WriteLine("--> Consuming User Created: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        var message = context.Message;
        var userName = message.UserName;
        var lastLoginTime = message.LastLoginTime;
        var roleCode = message.RoleCode;
        var isActive = message.IsActive;
        var version = message.Version;

        var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Code == roleCode);

        UserDto userDto = new()
        {
            UserName = userName,
            LastLoginTime = lastLoginTime,
            RoleId = role?.Id,
            IsActive = isActive,
            Version = version,
        };
        var user = mapper.Map<User>(userDto);
        dbContext.Users.Add(user);

        await dbContext.SaveChangesAsync();

        Console.WriteLine("--> Consumed User Created: " + context.MessageId);
    }
}
