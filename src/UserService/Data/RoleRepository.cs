using System;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using UserService.DTOs;
using UserService.Entities;

namespace UserService.Data;

public class RoleRepository(ApplicationDbContext context, IMapper mapper) : IRoleRepository
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    public async Task<Role?> GetRoleEntityById(Guid Id)
    {
        return await _context.Roles.FirstOrDefaultAsync(x => x.Id == Id);
    }

    public async Task<List<RoleDto>> GetRolesAsync()
    {
        var query = _context.Roles.AsQueryable();
        return await query.ProjectTo<RoleDto>(_mapper.ConfigurationProvider).ToListAsync();
    }
}
