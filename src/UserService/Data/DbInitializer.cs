using UserService.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Data;

public class DbInitializer
{
    public static void InitDb(WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var context = scope.ServiceProvider.GetService<ApplicationDbContext>()
            ?? throw new InvalidOperationException("Failed to retrieve ApplicationDbContext from the service provider.");

        SeedData(context);
    }

    private static void SeedData(ApplicationDbContext context)
    {
        context.Database.Migrate();
    }
}
