using System;
using UserService.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using UserService.Entities;
using UserService.Helpers;
using Microsoft.AspNetCore.Mvc;
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

    public async Task<PagedList<UserDto>> GetUsersPaginatedAsync(UserParams parameters)
    {
        Console.WriteLine(parameters.UserName_Like);
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(parameters.UserName_Like))
        {
            query = query.Where(s => s.UserName.Contains(parameters.UserName_Like));
        }

        return await PagedList<UserDto>.CreateAsync
            (query.ProjectTo<UserDto>(_mapper.ConfigurationProvider).AsNoTracking(),
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<bool> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync() > 0;
    }
}