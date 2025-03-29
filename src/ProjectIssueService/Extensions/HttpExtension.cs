using System.Text.Json;
using ProjectIssueService.Helpers;
using Microsoft.AspNetCore.Http;

namespace ProjectIssueService.Extensions
{
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int totalItems)
        {
            response.Headers.Append("x-total-count", totalItems.ToString());
            response.Headers.Append("Access-Control-Expose-Headers", "x-total-count");
        }

        public static string GetUserRole(this HttpContext context)
        {
            if (context.Items.TryGetValue("UserRole", out var userRoleObj) && userRoleObj != null)
            {
                return (string)userRoleObj;
            }

            return string.Empty;
        }

        public static Boolean CurrentUserRoleIsAdmin(this HttpContext context)
        {
            var UserRole = GetUserRole(context);
            return UserRole == UserRoles.Admin;
        }

        public static Boolean CurrentUserRoleIsMember(this HttpContext context)
        {
            var UserRole = GetUserRole(context);
            return UserRole == UserRoles.Member;
        }

        public static Boolean CurrentUserRoleIsViewer(this HttpContext context)
        {
            var UserRole = GetUserRole(context);
            return UserRole == UserRoles.Viewer;
        }
        public static string? GetCurrentUserName(this HttpContext context)
        {
            return context.User.Identity?.Name;
        }
    }
}