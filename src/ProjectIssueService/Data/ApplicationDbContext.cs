using ProjectIssueService.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace ProjectIssueService.Data;

public class ApplicationDbContext(DbContextOptions options, IHttpContextAccessor httpContextAccessor) : DbContext(options)
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<ProjectAssignment> ProjectAssignments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        // Configure unique constraint for project assignment
        modelBuilder.Entity<ProjectAssignment>()
            .HasIndex(p => new { p.ProjectId, p.UserName }).IsUnique();
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
        // Console.WriteLine(currentUsername);

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