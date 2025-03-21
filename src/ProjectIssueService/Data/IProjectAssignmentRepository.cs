using System;
using ProjectIssueService.DTOs;
using ProjectIssueService.Entities;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Data;

public interface IProjectAssignmentRepository
{
    Task<List<ProjectAssignmentDto>> GetProjectAssignmentsAsync(ProjectAssignmentParams parameters);
    Task<PagedList<ProjectAssignmentDto>> GetProjectAssignmentsPaginatedAsync(ProjectAssignmentParams parameters);
    Task<ProjectAssignmentDto?> GetProjectAssignmentByIdAsync(Guid id);
    Task<ProjectAssignment?> GetProjectAssignmentEntityById(Guid id);
    Task<IEnumerable<ProjectAssignment>> GetProjectAssignmentsByProjectIdAsync(Guid projectId);
    void AddProjectAssignment(ProjectAssignment projectAssignment);
    void RemoveProjectAssignment(ProjectAssignment projectAssignment);
    Task<bool> SaveChangesAsync();
    Task<ProjectAssignmentDto?> GetProjectAssignmentByProjectIdAndUserNameAsync(Guid projectId, string userName);
}
