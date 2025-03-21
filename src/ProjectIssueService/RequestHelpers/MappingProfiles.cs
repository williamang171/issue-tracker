using AutoMapper;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using Contracts;
namespace ProjectIssueService.RequestHelpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Project mappings
        CreateMap<Project, ProjectDto>();
        CreateMap<ProjectDto, Project>();
        CreateMap<ProjectCreateDto, Project>();
        CreateMap<Project, ProjectCreateDto>();
        CreateMap<ProjectUpdateDto, Project>();
        CreateMap<Project, ProjectUpdateDto>();
        CreateMap<ProjectDto, ProjectDeleted>();
        CreateMap<Project, ProjectForSelectDto>();

        // Issue mappings
        CreateMap<Issue, IssueDto>();
        CreateMap<IssueDto, Issue>();
        CreateMap<IssueCreateDto, Issue>();
        CreateMap<Issue, IssueCreateDto>();
        CreateMap<IssueUpdateDto, Issue>();
        CreateMap<Issue, IssueUpdateDto>();
        CreateMap<IssueDto, IssueCreated>();
        CreateMap<IssueDto, IssueValues>();
        CreateMap<IssueDto, IssueDeleted>();

        // Project Assignment mappings
        CreateMap<ProjectAssignment, ProjectAssignmentDto>();
        CreateMap<ProjectAssignmentDto, ProjectAssignment>();
        CreateMap<ProjectAssignmentCreateDto, ProjectAssignment>();
        CreateMap<ProjectAssignment, BulkProjectAssignmentDto>();

        // User mappings
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();
        CreateMap<UserDto, UserCreated>();
    }
}