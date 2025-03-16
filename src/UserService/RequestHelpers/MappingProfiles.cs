using AutoMapper;
using UserService.DTOs;
using UserService.Entities;
namespace UserService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();
        CreateMap<UserSyncLastLoginDto, User>();
    }
}