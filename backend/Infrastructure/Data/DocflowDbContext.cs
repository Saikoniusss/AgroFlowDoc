using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class DocflowDbContext : DbContext
{
    public DocflowDbContext(DbContextOptions<DocflowDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Process> Processes => Set<Process>();
    public DbSet<DocumentTemplate> DocumentTemplates => Set<DocumentTemplate>();
    public DbSet<DocumentTemplateField> DocumentTemplateFields => Set<DocumentTemplateField>();
    public DbSet<WorkflowRoute> WorkflowRoutes => Set<WorkflowRoute>();
    public DbSet<WorkflowRouteStep> WorkflowRouteSteps => Set<WorkflowRouteStep>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<WFTracker> WFTrackers => Set<WFTracker>();
    public DbSet<DocumentFile> DocumentFiles => Set<DocumentFile>();
    public DbSet<DocumentComment> DocumentComments => Set<DocumentComment>();
    public DbSet<DocumentActionLog> DocumentActionLogs => Set<DocumentActionLog>();

 protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.Username).HasMaxLength(128).IsRequired();
            e.Property(x => x.PasswordHash).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
        });

        modelBuilder.Entity<Role>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).HasMaxLength(128).IsRequired();
        });

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

         // --- Process ---
        modelBuilder.Entity<Process>()
            .HasOne(x => x.DocumentTemplate)
            .WithMany(t => t.Processes)
            .HasForeignKey(x => x.DocumentTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Process>()
            .HasOne(x => x.WorkflowRoute)
            .WithMany(r => r.Processes)
            .HasForeignKey(x => x.WorkflowRouteId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- Document ---
        modelBuilder.Entity<Document>()
            .HasOne(x => x.CurrentStep)
            .WithMany()
            .HasForeignKey(x => x.CurrentStepId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(x => x.Template)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .HasOne(x => x.Process)
            .WithMany(p => p.Documents)
            .HasForeignKey(x => x.ProcessId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- WFTracker ---
        modelBuilder.Entity<WFTracker>()
            .HasOne(x => x.Document)
            .WithMany(d => d.WorkflowTrackers)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<WFTracker>()
            .HasIndex(x => new { x.DocumentId, x.StepOrder });

        // --- DocumentFile ---
        modelBuilder.Entity<DocumentFile>()
            .HasOne(x => x.Document)
            .WithMany(d => d.Files)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- DocumentComment ---
        modelBuilder.Entity<DocumentComment>()
            .HasOne(x => x.Document)
            .WithMany(d => d.Comments)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);

        // --- DocumentActionLog ---
        modelBuilder.Entity<DocumentActionLog>()
            .HasOne(x => x.Document)
            .WithMany(d => d.Actions)
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.Restrict);
    }   
}