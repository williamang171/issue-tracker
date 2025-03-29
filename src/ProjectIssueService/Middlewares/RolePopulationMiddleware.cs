using Microsoft.EntityFrameworkCore;
using ProjectIssueService.Data;

namespace ProjectIssueService.Middlewares;

public class RolePopulationMiddleware(RequestDelegate _next)
{
    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        var currentUsername = context.User?.Identity?.Name;
        if (currentUsername != null)
        {
            var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == currentUsername);
            if (user != null)
            {
                var role = await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == user.RoleId);
                context.Items["UserRole"] = role?.Code;
            }
        }
        await _next(context);
    }
}