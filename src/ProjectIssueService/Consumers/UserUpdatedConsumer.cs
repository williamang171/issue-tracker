using System;
using System.Text.Json;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Data;

namespace ProjectIssueService.Consumers;

public class UserUpdatedConsumer(ApplicationDbContext dbContext) : IConsumer<UserUpdated>
{
    public async Task Consume(ConsumeContext<UserUpdated> context)
    {
        Console.WriteLine("--> Consuming User Updated: " + context.MessageId);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        var message = context.Message;
        var userName = message.UserName;
        var oldValues = message.OldValues;
        var newValues = message.NewValues;
        var oldVersion = message.OldVersion;
        var newVersion = message.NewVersion;

        var user = await dbContext
            .Users
            .FirstOrDefaultAsync(x => x.UserName == userName && x.Version.Equals(oldVersion));

        if (user == null)
        {
            throw new MessageException(typeof(UserUpdated), $"User with UserName:{userName} and Version:{oldVersion} not found");
        }

        if (newValues.RoleCode != null)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Code == newValues.RoleCode);
            if (role == null)
            {
                throw new MessageException(typeof(UserUpdated), $"Role with Code:{newValues.RoleCode} not found");
            }
            user.RoleId = role.Id;
        }
        user.IsActive = newValues.IsActive ?? user.IsActive;
        user.Version = newVersion;

        await dbContext.SaveChangesAsync();

        Console.WriteLine("--> Consumed User Updated: " + context.MessageId);
    }
}
