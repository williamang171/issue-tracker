using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.DTOs;
using UserService.Entities;

namespace UserService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserRepository repo, IMapper mapper, IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        private readonly IUserRepository _userRepo = repo;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var response = await _userRepo.GetUsersAsync();
            return response;
        }

        [HttpPost]
        [Route("sync-last-login")]
        public async Task<ActionResult> SyncLastLogin()
        {
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (currentUsername == null)
            {
                return BadRequest("currentUsername is null");
            }
            var user = await _userRepo.GetUserEntityByUserName(currentUsername);

            if (user != null)
            {
                user.LastLoginTime = DateTime.UtcNow;
            }
            else
            {
                var newUserDto = new UserSyncLastLoginDto
                {
                    UserName = currentUsername,
                };
                var newUser = _mapper.Map<User>(newUserDto);
                newUser.LastLoginTime = DateTime.UtcNow;
                _userRepo.AddUser(newUser);
            }
            if (await _userRepo.SaveChangesAsync())
            {
                return Ok();
            }
            return BadRequest("Failed to update user:sync-last-login");
        }
    }
}
