using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.DTOs;

namespace UserService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController(
    IRoleRepository roleRepo) : ControllerBase
    {
        private readonly IRoleRepository _roleRepo = roleRepo;

        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetRoles()
        {
            var response = await _roleRepo.GetRolesAsync();
            return response;
        }
    }
}
