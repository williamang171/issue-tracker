using System;
using UserService.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using UserService.Entities;
namespace UserService.Data;

public class UserRepository(ApplicationDbContext context, IMapper mapper) : IUserRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    public void AddUser(User user)
    {
        _context.Users.Add(user);
    }
    public async Task<UserDto?> GetUserByUserNameAsync(string userName)
    {
        return await _context.Users
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(x => x.UserName == userName);
    }

    public async Task<User?> GetUserEntityByUserName(string userName)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.UserName == userName);
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var query = _context.Users.AsQueryable();
        return await query.ProjectTo<UserDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}