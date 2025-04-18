using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace IdentityService;

public class SeedData
{
    public static async Task EnsureSeedData(WebApplication app)
    {
        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (userMgr.Users.Any()) return;

        await SeedUsersAsync(userMgr, app);
    }

    public static async Task SeedUsersAsync(UserManager<ApplicationUser> userMgr, WebApplication app)
    {
        // Define the users to create
        var usersToCreate = new List<(string Username, string Email, string DisplayName)>
    {
        ("root", "root@email.com", "Root"),
        ("alice", "AliceSmith@email.com", "Alice Smith"),
        ("bob", "BobSmith@email.com", "Bob Smith"),
        ("demoadmin", "demoadmin@example.com", "Demo Admin"),
        ("demomember", "demomember@example.com", "Demo Member"),
        ("demoviewer", "demoviewer@example.com", "Demo Viewer")
    };
        var defaultPassword = app.Configuration["UserSettings:DefaultPassword"] ?? "Pass123$";

        // Create or verify each user
        foreach (var (username, email, displayName) in usersToCreate)
        {
            await EnsureUserExistsAsync(userMgr, username, email, displayName, defaultPassword);
        }
    }

    private static async Task EnsureUserExistsAsync(
  UserManager<ApplicationUser> userMgr,
  string username,
  string email,
  string displayName,
  string defaultPassword)
    {
        var user = await userMgr.FindByNameAsync(username);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = username,
                Email = email,
                EmailConfirmed = true
            };

            var result = await userMgr.CreateAsync(user, defaultPassword);
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            result = await userMgr.AddClaimsAsync(user, new[]
            {
            new Claim(JwtClaimTypes.Name, displayName)
        });

            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }

            Log.Debug($"{username} created");
        }
        else
        {
            Log.Debug($"{username} already exists");
        }
    }

}
