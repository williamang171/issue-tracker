using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public interface IProjectAssignmentRepository
{
    Task<List<ProjectAssignmentDto>> GetProjectAssignmentsAsync();
    Task<PagedList<ProjectAssignmentDto>> GetProjectAssignmentsPaginatedAsync(PaginationParams parameters);
    Task<ProjectAssignmentDto?> GetProjectAssignmentByIdAsync(Guid id);
    Task<ProjectAssignment?> GetProjectAssignmentEntityById(Guid id);
    void AddProjectAssignment(ProjectAssignment projectAssignment);
    void RemoveProjectAssignment(ProjectAssignment projectAssignment);
    Task<bool> SaveChangesAsync();
    Task<ProjectAssignmentDto?> GetProjectAssignmentByProjectIdAndUserNameAsync(Guid projectId, string userName);
}
