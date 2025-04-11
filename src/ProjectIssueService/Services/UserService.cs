using System;
using ProjectIssueService.Helpers;

namespace ProjectIssueService.Services;

public class UserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public bool CurrentUserRoleIsAdmin()
    {
        var UserRole = GetUserRole();
        return UserRole == UserRoles.Admin;
    }

    public bool CurrentUserRoleIsMember()
    {
        var UserRole = GetUserRole();
        return UserRole == UserRoles.Member;
    }

    public bool CurrentUserRoleIsViewer()
    {
        var UserRole = GetUserRole();
        return UserRole == UserRoles.Viewer;
    }

    public string? GetCurrentUserName()
    {
        var context = httpContextAccessor.HttpContext;
        if (context != null)
        {
            return context.User.Identity?.Name;
        }
        return null;
    }

    public string GetUserRole()
    {
        var context = httpContextAccessor.HttpContext;
        if (context != null && context.Items.TryGetValue("UserRole", out var userRoleObj) && userRoleObj != null)
        {
            return (string)userRoleObj;
        }
        return string.Empty;
    }
}
