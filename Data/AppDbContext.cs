using APM.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace APM.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // === TABLES ===
        public DbSet<User> Users => Set<User>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Team> Teams => Set<Team>();
        public DbSet<Process> Processes => Set<Process>();
        public DbSet<ActionPlan> ActionPlans => Set<ActionPlan>();
        public DbSet<ActionItem> ActionItems => Set<ActionItem>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === USER ===
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(50);

                // Chef hiérarchique (self-reference)
                entity.HasOne(e => e.Manager)
                      .WithMany(e => e.Subordinates)
                      .HasForeignKey(e => e.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                      .WithMany(e => e.Users)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(e => e.Team)
                      .WithMany(e => e.Members)
                      .HasForeignKey(e => e.TeamId)
                      .OnDelete(DeleteBehavior.NoAction); 
            });

            // === DEPARTMENT ===
            modelBuilder.Entity<Department>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            });

            // === TEAM ===
            modelBuilder.Entity<Team>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

                entity.HasOne(e => e.Department)
                      .WithMany(e => e.Teams)
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // === PROCESS ===
            modelBuilder.Entity<Process>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            });

            // === ACTION PLAN ===
            modelBuilder.Entity<ActionPlan>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);

                entity.HasOne(e => e.Pilot)
                      .WithMany(e => e.ManagedPlans)
                      .HasForeignKey(e => e.PilotId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Process)
                      .WithMany()
                      .HasForeignKey(e => e.ProcessId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Department)
                      .WithMany()
                      .HasForeignKey(e => e.DepartmentId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Many-to-Many pour les co-pilotes
                entity.HasMany(e => e.CoPilots)
                      .WithMany(e => e.CoManagedPlans)
                      .UsingEntity(j => j.ToTable("ActionPlanCoPilots"));
            });

            // === ACTION ITEM ===
            modelBuilder.Entity<ActionItem>(entity =>
            {
                entity.Property(e => e.Theme).IsRequired().HasMaxLength(300);
                entity.Property(e => e.ActionDescription).IsRequired();

                entity.HasOne(e => e.ActionPlan)
                      .WithMany(e => e.Actions)
                      .HasForeignKey(e => e.ActionPlanId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Responsible)
                      .WithMany(e => e.AssignedActions)
                      .HasForeignKey(e => e.ResponsibleId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Action parent (remplacement si inefficace)
                entity.HasOne(e => e.ParentAction)
                      .WithMany(e => e.ChildActions)
                      .HasForeignKey(e => e.ParentActionId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === COMMENT ===
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.Content).IsRequired();

                entity.HasOne(e => e.ActionItem)
                      .WithMany(e => e.Comments)
                      .HasForeignKey(e => e.ActionItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Author)
                      .WithMany()
                      .HasForeignKey(e => e.AuthorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === ATTACHMENT ===
            modelBuilder.Entity<Attachment>(entity =>
            {
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(500);
                entity.Property(e => e.FilePath).IsRequired();

                entity.HasOne(e => e.ActionItem)
                      .WithMany(e => e.Attachments)
                      .HasForeignKey(e => e.ActionItemId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UploadedBy)
                      .WithMany()
                      .HasForeignKey(e => e.UploadedById)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // === NOTIFICATION ===
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Message).IsRequired();

                entity.HasOne(e => e.User)
                      .WithMany(e => e.Notifications)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // === NOTIFICATION LOG ===
            modelBuilder.Entity<NotificationLog>(entity =>
            {
                entity.HasOne(e => e.ActionItem)
                      .WithMany()
                      .HasForeignKey(e => e.ActionItemId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // === ACTIVITY LOG ===
            modelBuilder.Entity<ActivityLog>(entity =>
            {
                entity.HasOne(e => e.User)
                      .WithMany(e => e.ActivityLogs)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}