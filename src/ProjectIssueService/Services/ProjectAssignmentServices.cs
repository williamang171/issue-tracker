using System;
using ProjectIssueService.Data;
using ProjectIssueService.Extensions;

namespace ProjectIssueService.Services;

public class ProjectAssignmentServices(IProjectAssignmentRepository repo, IHttpContextAccessor httpContextAccessor) : IProjectAssignmentServices
{
    public async Task<bool> CanCurrentUserAccessProject(Guid? projectId)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var isAdmin = httpContext?.CurrentUserRoleIsAdmin();

        // If user is an admin, can access all projects
        if (isAdmin != null && isAdmin == true) return true;

        // If userName not found, return false
        var userName = httpContext?.GetCurrentUserName();
        if (projectId == null || string.IsNullOrEmpty(userName)) return false;

        // Only return true if user is assigned to given project
        var assignment = await repo.GetProjectAssignmentByProjectIdAndUserNameAsync(projectId.Value, userName);
        return assignment != null;
    }

    public async Task<bool> IsUserAssignedToProject(Guid? projectId, string? userName)
    {
        // If userName not found, return false
        if (projectId == null || string.IsNullOrEmpty(userName)) return false;

        // Only return true if user is assigned to given project
        var assignment = await repo.GetProjectAssignmentByProjectIdAndUserNameAsync(projectId.Value, userName);
        return assignment != null;
    }
}
