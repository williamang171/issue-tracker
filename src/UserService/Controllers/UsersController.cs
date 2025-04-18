using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.DTOs;
using UserService.Entities;
using MassTransit;
using Contracts;
using UserService.Helpers;
using UserService.Extensions;

namespace UserService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserRepository repo,
    IRoleRepository roleRepo,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    IPublishEndpoint publishEndpoint) : ControllerBase
    {
        private readonly IUserRepository _userRepo = repo;
        private readonly IRoleRepository _roleRepo = roleRepo;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IMapper _mapper = mapper;

        [HttpGet("getCurrentUserRole")]
        public ActionResult<CurrentUserRoleDto> GetCurrentUserRole()
        {
            var userRole = HttpContext.GetUserRole();
            return new CurrentUserRoleDto()
            {
                RoleCode = userRole
            };
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetUsers([FromQuery] UserParams parameters)
        {
            if (parameters.Pagination.HasValue && parameters.Pagination == false)
            {
                return await _userRepo.GetUsersAsync();
            }
            var response = await _userRepo.GetUsersPaginatedAsync(parameters);
            Response.AddPaginationHeader(response.TotalCount);
            return response;
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<UserDto>> GetUserByUserName(string username)
        {
            var response = await _userRepo.GetUserByUserNameAsync(username);
            if (response == null)
            {
                return NotFound();
            }
            return response;
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{username}")]
        public async Task<ActionResult> UpdateUser(string username, UserUpdateDto dto)
        {
            var user = await _userRepo.GetUserEntityByUserName(username);
            if (user == null)
            {
                return NotFound();
            }

            // Root user cannot be updated
            if (user.IsRoot)
            {
                return Forbid();
            }

            var roleId = dto.RoleId;
            string? roleCode = null;
            if (roleId.HasValue)
            {
                var role = await _roleRepo.GetRoleEntityById(roleId.Value);
                if (role == null)
                {
                    return BadRequest("Role not found");
                }
                roleCode = role.Code;
            }

            var oldUserDto = _mapper.Map<UserDto>(user);
            var newVersion = Guid.NewGuid();
            var oldVersion = oldUserDto.Version;

            user.RoleId = dto.RoleId ?? user.RoleId;
            user.IsActive = dto.IsActive ?? user.IsActive;
            user.Version = newVersion;

            var newUserDto = _mapper.Map<UserDto>(user);
            if (roleCode != null)
            {
                newUserDto.RoleCode = roleCode;
            }

            UserUpdated userUpdated = new()
            {
                Id = user.Id,
                UserName = user.UserName,
                OldValues = _mapper.Map<UserValues>(oldUserDto),
                NewValues = _mapper.Map<UserValues>(newUserDto),
                OldVersion = oldVersion,
                NewVersion = newVersion,
            };
            await publishEndpoint.Publish(userUpdated);

            await _userRepo.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        [Route("sync")]
        public async Task<ActionResult> Sync()
        {
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (currentUsername == null)
            {
                return BadRequest("currentUsername is null");
            }
            var user = await _userRepo.GetUserEntityByUserName(currentUsername);

            // Update existing user
            if (user != null)
            {
                user.LastLoginTime = DateTime.UtcNow;
            }
            // Create a new user
            else
            {
                var viewerRole = await GetDefaultRoleForUserName(currentUsername);
                var newUserDto = new UserSyncDto
                {
                    UserName = currentUsername,
                    LastLoginTime = DateTime.UtcNow,
                    RoleId = viewerRole?.Id,
                    RoleCode = viewerRole?.Code,
                    IsActive = true,
                    Version = Guid.NewGuid(),
                    IsRoot = currentUsername == "root"
                };
                var newUser = _mapper.Map<User>(newUserDto);
                _userRepo.AddUser(newUser);
                var toPublish = _mapper.Map<UserCreated>(newUser);
                toPublish.RoleCode = viewerRole?.Code;
                await publishEndpoint.Publish(_mapper.Map<UserCreated>(newUser));
            }
            if (await _userRepo.SaveChangesAsync())
            {
                return Ok();
            }
            return BadRequest("Failed to update user:sync");
        }

        // Default role for demo users
        private async Task<Role?> GetDefaultRoleForUserName(string userName)
        {
            List<string> adminList = ["alice", "root", "demoadmin"];
            if (adminList.Contains(userName))
            {
                var adminRole = await _roleRepo.GetRoleEntityByCode("Admin");
                return adminRole;
            }

            List<string> memberList = ["demomember", "bob"];
            if (memberList.Contains(userName))
            {
                var memberRole = await _roleRepo.GetRoleEntityByCode("Member");
                return memberRole;
            }

            var viewerRole = await _roleRepo.GetRoleEntityByCode("Viewer");
            return viewerRole;
        }
    }
}