using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Entities;

namespace UserService.Data;

public interface IRoleRepository
{
    Task<List<RoleDto>> GetRolesAsync();
    Task<Role?> GetRoleEntityById(Guid Id);
    Task<Role?> GetRoleEntityByCode(string Code);
}