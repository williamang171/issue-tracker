using System;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Services;

public interface IUserService
{
    public string GetUserRole();
    public bool CurrentUserRoleIsAdmin();
    public bool CurrentUserRoleIsMember();
    public bool CurrentUserRoleIsViewer();
    public string? GetCurrentUserName();
}
