using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace UserService.Extensions
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
    }
}