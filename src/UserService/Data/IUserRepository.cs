using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Entities;
using UserService.Helpers;

namespace UserService.Data;

public interface IUserRepository
{
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserByUserNameAsync(string userName);
    Task<PagedList<UserDto>> GetUsersPaginatedAsync(UserParams parameters);
    Task<User?> GetUserEntityByUserName(string userName);
    Task<bool> SaveChangesAsync();
    void AddUser(User user);
}