using UserService.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>()
            ?? throw new InvalidOperationException("Failed to retrieve ApplicationDbContext from the service provider.");

        await SeedData(context);
    }

    private static async Task SeedData(ApplicationDbContext context)
    {
        context.Database.Migrate();

        var rolesCount = await context.Roles.CountAsync();
        if (rolesCount == 0)
        {
            var roles = new List<Role>
            {
                new() {
                    Name = "Admin",
                    Code = "Admin",
                    CreatedBy = "System",
                    Version = Guid.NewGuid(),
                },
                new() {
                    Name = "Member",
                    Code = "Member",
                    CreatedBy = "System",
                    Version = Guid.NewGuid(),
                },
                new() {
                    Name = "Viewer",
                    Code = "Viewer",
                    CreatedBy = "System",
                    Version = Guid.NewGuid(),
                },
            };

            try
            {
                // Ensure database is created
                context.Database.EnsureCreated();

                // Check if roles already exist to avoid duplicates
                var existingRoles = context.Roles.Select(r => r.Name).ToList();

                // Filter out roles that already exist
                var newRoles = roles.Where(r => !existingRoles.Contains(r.Name)).ToList();

                if (newRoles.Count != 0)
                {
                    // Add the new roles
                    context.Roles.AddRange(newRoles);
                    context.SaveChanges();
                    Console.WriteLine($"Successfully added {newRoles.Count} roles to the database.");

                    // Display the added roles
                    foreach (var role in newRoles)
                    {
                        Console.WriteLine($"Added role: {role.Name}");
                    }
                }
                else
                {
                    Console.WriteLine("No new roles to add. All roles already exist in the database.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding roles to database: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }

        }
    }
}
