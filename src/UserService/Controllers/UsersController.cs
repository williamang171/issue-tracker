using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Data;
using UserService.DTOs;
using UserService.Entities;
using MassTransit;
using Contracts;

namespace UserService.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController(IUserRepository repo,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    IPublishEndpoint publishEndpoint) : ControllerBase
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
        [Route("sync")]
        public async Task<ActionResult> Sync()
        {
            var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (currentUsername == null)
            {
                return BadRequest("currentUsername is null");
            }
            var user = await _userRepo.GetUserEntityByUserName(currentUsername);

            if (user != null)
            {
                var oldUserDto = _mapper.Map<UserDto>(user);
                user.LastLoginTime = DateTime.UtcNow;
                var newUserDto = _mapper.Map<UserDto>(user);
                UserUpdated userUpdated = new()
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    OldValues = _mapper.Map<UserValues>(oldUserDto),
                    NewValues = _mapper.Map<UserValues>(newUserDto),
                };
                await publishEndpoint.Publish(userUpdated);
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
                var toPublish = _mapper.Map<UserDto>(newUser);
                await publishEndpoint.Publish(_mapper.Map<UserCreated>(toPublish));
            }
            if (await _userRepo.SaveChangesAsync())
            {
                return Ok();
            }
            return BadRequest("Failed to update user:sync");
        }
    }
}
