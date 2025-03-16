using UserService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace UserService.Data;

public class ApplicationDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        SetModifiedInformation();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        SetModifiedInformation();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SetModifiedInformation()
    {
        var currentUsername = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        Console.WriteLine(currentUsername);

        var entries = ChangeTracker
        .Entries()
        .Where(e => e.Entity is BaseEntity && (
            e.State == EntityState.Added ||
            e.State == EntityState.Modified));

        foreach (var entityEntry in entries)
        {
            if (entityEntry.State == EntityState.Added)
            {
                ((BaseEntity)entityEntry.Entity).CreatedBy = currentUsername;
                ((BaseEntity)entityEntry.Entity).CreatedTime = DateTime.UtcNow;
            }
            if (entityEntry.State == EntityState.Modified)
            {
                ((BaseEntity)entityEntry.Entity).UpdatedBy = currentUsername;
                ((BaseEntity)entityEntry.Entity).UpdatedTime = DateTime.UtcNow;
            }
        }
    }
}