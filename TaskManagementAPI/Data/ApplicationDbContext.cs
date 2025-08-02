using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // DbSets
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<OrganizationMember> OrganizationMembers { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Models.Entities.TaskStatus> TaskStatuses { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskAttachment> TaskAttachments { get; set; }
        public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships and constraints
            ConfigureUserRelationships(builder);
            ConfigureOrganizationRelationships(builder);
            ConfigureTaskRelationships(builder);
            ConfigureIndexes(builder);
        }

        private void ConfigureUserRelationships(ModelBuilder builder)
        {
            // TaskItem relationships
            builder.Entity<TaskItem>()
                .HasOne(t => t.AssignedToUser)
                .WithMany(u => u.AssignedTasks)
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<TaskItem>()
                .HasOne(t => t.CreatedByUser)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private void ConfigureOrganizationRelationships(ModelBuilder builder)
        {
            // Organization cascading deletes
            builder.Entity<OrganizationMember>()
                .HasOne(om => om.Organization)
                .WithMany(o => o.Members)
                .HasForeignKey(om => om.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Team>()
                .HasOne(t => t.Organization)
                .WithMany(o => o.Teams)
                .HasForeignKey(t => t.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Project>()
                .HasOne(p => p.Organization)
                .WithMany(o => o.Projects)
                .HasForeignKey(p => p.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureTaskRelationships(ModelBuilder builder)
        {
            // Fix cascade delete conflicts

            // TaskStatus relationship - Change to NoAction to prevent cascade cycle
            builder.Entity<TaskItem>()
                .HasOne(t => t.TaskStatus)
                .WithMany(ts => ts.Tasks)
                .HasForeignKey(t => t.TaskStatusId)
                .OnDelete(DeleteBehavior.NoAction); // Changed from Cascade to NoAction

            // Project relationship - Keep cascade (this is the primary delete path)
            builder.Entity<TaskItem>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Task comment self-referencing relationship
            builder.Entity<TaskComment>()
                .HasOne(tc => tc.ParentComment)
                .WithMany(tc => tc.Replies)
                .HasForeignKey(tc => tc.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // TaskComment to Task relationship - NoAction to prevent cascade conflicts
            builder.Entity<TaskComment>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.NoAction);

            // TaskAttachment to Task relationship - NoAction to prevent cascade conflicts
            builder.Entity<TaskAttachment>()
                .HasOne(ta => ta.Task)
                .WithMany(t => t.Attachments)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.NoAction);

            // TimeEntry to Task relationship - NoAction to prevent cascade conflicts
            builder.Entity<TimeEntry>()
                .HasOne(te => te.Task)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(te => te.TaskId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        private void ConfigureIndexes(ModelBuilder builder)
        {
            // Performance indexes
            builder.Entity<TaskItem>()
                .HasIndex(t => new { t.ProjectId, t.TaskStatusId });

            builder.Entity<TaskItem>()
                .HasIndex(t => t.AssignedToUserId);

            builder.Entity<TaskItem>()
                .HasIndex(t => t.DueDate);

            builder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead });

            builder.Entity<TimeEntry>()
                .HasIndex(te => new { te.UserId, te.Date });
        }

    }
}
