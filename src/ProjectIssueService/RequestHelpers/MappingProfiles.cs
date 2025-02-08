using AutoMapper;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
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

        // Issue mappings
        CreateMap<Issue, IssueDto>();
        CreateMap<IssueDto, Issue>();
        CreateMap<IssueCreateDto, Issue>();
        CreateMap<Issue, IssueCreateDto>();
        CreateMap<IssueUpdateDto, Issue>();
        CreateMap<Issue, IssueUpdateDto>();
    }
}