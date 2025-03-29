using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace ProjectIssueService.AuthorizationHandler;

public class CustomRoleHandler : AuthorizationHandler<RolesAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RolesAuthorizationRequirement requirement)
    {
        var isAuthenticated = context.User.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            return Task.CompletedTask;
        }

        var httpContext = context.Resource as HttpContext;
        if (httpContext != null &&
            httpContext.Items.TryGetValue("UserRole", out var userRoleObj) &&
            userRoleObj is string userRole)
        {
            // Check if the user's role is in the required roles list
            if (requirement.AllowedRoles.Any(role => role.Equals(userRole, StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
            }
        }

        return Task.CompletedTask;
    }
}
