using System;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Entities;

namespace ProjectIssueService.Data;

public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetUserEntityByUserName(string UserName)
    {
        return await context.Users.FirstOrDefaultAsync(x => x.UserName == UserName);
    }
}
