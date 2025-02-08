using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;

namespace ProjectIssueService.Data;

public interface IProjectRepository
{
    Task<List<ProjectDto>> GetProjectsAsync();
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<Project?> GetProjectEntityById(Guid id);
    void AddProject(Project project);
    void RemoveProject(Project project);
    Task<bool> SaveChangesAsync();
}
