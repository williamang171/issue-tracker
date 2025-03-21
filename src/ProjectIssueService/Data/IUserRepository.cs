using System;
using ProjectIssueService.Entities;

namespace ProjectIssueService.Data;

public interface IUserRepository
{
    Task<User?> GetUserEntityByUserName(string UserName);
}
