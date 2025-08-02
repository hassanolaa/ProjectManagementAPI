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
            // Task comment self-referencing relationship
            builder.Entity<TaskComment>()
                .HasOne(tc => tc.ParentComment)
                .WithMany(tc => tc.Replies)
                .HasForeignKey(tc => tc.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
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
