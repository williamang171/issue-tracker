using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text.Json;
using UserService.Data;
using Microsoft.EntityFrameworkCore;

namespace UserService.Middlewares;
public class UserIsActiveMiddleware(RequestDelegate _next)
{
  public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
  {
    var userName = context.User?.Identity?.Name;
    if (!string.IsNullOrEmpty(userName))
    {
      // Get the user from the service
      var user = await dbContext.Users.FirstOrDefaultAsync(x => x.UserName == userName);

      // Check if user exists and is active
      if (user == null || !user.IsActive)
      {
        // User is not active, return an error response
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        context.Response.ContentType = "application/json";

        var response = new
        {
          Status = "Error",
          Message = "User account is inactive. Please contact support."
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        return;
      }
    }
    await _next(context);
  }
}