using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Entities;

namespace UserService.Data;

public interface IUserRepository
{
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserByUserNameAsync(string userName);
    Task<User?> GetUserEntityByUserName(string userName);
    Task<bool> SaveChangesAsync();
    void AddUser(User user);
}