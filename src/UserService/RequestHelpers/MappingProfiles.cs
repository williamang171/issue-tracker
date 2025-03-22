using AutoMapper;
using Contracts;
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
        CreateMap<UserDto, UserCreated>();
        CreateMap<UserDto, UserValues>();

        // Role mappings
        CreateMap<Role, RoleDto>();
        CreateMap<RoleDto, RoleDto>();
    }
}