using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public interface IProjectRepository
{
    Task<List<ProjectDto>> GetProjectsAsync();
    Task<List<ProjectForSelectDto>> GetProjectsForSelectAsync();
    Task<PagedList<ProjectDto>> GetProjectsPaginatedAsync(ProjectParams parameters);
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<Project?> GetProjectEntityById(Guid id);
    void AddProject(Project project);
    void RemoveProject(Project project);
    Task<bool> SaveChangesAsync();
}
