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
        Console.WriteLine("--> Consuming User Updated: " + context.Message.Id);

        // For Debugging
        var messageJsonString = JsonSerializer.Serialize(context.Message, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(messageJsonString);

        var message = context.Message;
        var userName = message.UserName;
        var oldValues = message.OldValues;
        var newValues = message.NewValues;

        var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userName);

        if (user == null)
        {
            Console.WriteLine("UserUpdatedConsumer: user not found");
            return;
        }

        if (newValues.RoleCode != null)
        {
            var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Code == newValues.RoleCode);
            if (role == null)
            {
                Console.WriteLine("UserUpdatedConsumer: role not found");
                return;
            }
            user.RoleId = role.Id;
        }
        user.IsActive = newValues.IsActive ?? user.IsActive;

        await dbContext.SaveChangesAsync();

        Console.WriteLine("--> Consumed User Updated: " + context.Message.Id);
    }
}
