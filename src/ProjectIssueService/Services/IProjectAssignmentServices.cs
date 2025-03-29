using System;

namespace ProjectIssueService.Services;

public interface IProjectAssignmentServices
{
    Task<bool> CanCurrentUserAccessProject(Guid? projectId);
    Task<bool> IsUserAssignedToProject(Guid? projectId, string? userName);
}

