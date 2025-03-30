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
        CreateMap<User, UserCreated>();
        CreateMap<UserDto, User>();
        CreateMap<UserSyncDto, User>();
        CreateMap<UserDto, UserCreated>();
        CreateMap<UserDto, UserValues>();

        // Role mappings
        CreateMap<Role, RoleDto>();
        CreateMap<RoleDto, RoleDto>();
    }
}