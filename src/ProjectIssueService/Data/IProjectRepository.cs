using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public interface IProjectRepository
{
    Task<List<ProjectDto>> GetProjectsAsync();
    Task<List<ProjectForSelectDto>> GetProjectsForSelectAsync(string? userName);
    Task<PagedList<ProjectDto>> GetProjectsPaginatedAsync(ProjectParams parameters, string? projectAssignee);
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<ProjectDto?> GetProjectByIdAndProjectAssigneeAsync(Guid id, string projectAssignee);
    Task<Project?> GetProjectEntityById(Guid id);
    void AddProject(Project project);
    void RemoveProject(Project project);
    Task<bool> SaveChangesAsync();
}
