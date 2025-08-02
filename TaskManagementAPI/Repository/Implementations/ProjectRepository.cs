using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Repository.Interfaces;

namespace TaskManagementAPI.Repository.Implementations
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        public ProjectRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Project>> GetByOrganizationAsync(int organizationId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.OrganizationId == organizationId && p.IsActive)
                .Include(p => p.Team)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetByTeamAsync(int teamId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.TeamId == teamId && p.IsActive)
                .ToListAsync();
        }

        public async Task<IEnumerable<Project>> GetUserProjectsAsync(string userId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.Members.Any(m => m.UserId == userId && m.IsActive) && p.IsActive)
                .Include(p => p.Organization)
                .Include(p => p.Team)
                .ToListAsync();
        }

        public async Task<Project?> GetWithTasksAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.TaskStatus)
                .Include(p => p.Tasks)
                    .ThenInclude(t => t.AssignedToUser)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<Project?> GetWithMembersAsync(int projectId)
        {
            return await _context.Projects
                .Include(p => p.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(p => p.Id == projectId);
        }

        public async Task<bool> IsUserMemberAsync(int projectId, string userId)
        {
            return await _context.ProjectMembers
                .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive);
        }

        public async Task<string?> GetUserRoleAsync(int projectId, string userId)
        {
            var member = await _context.ProjectMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive);
            
            return member?.Role;
        }

        public async Task<IEnumerable<Project>> GetActiveProjectsAsync(int organizationId)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(p => p.OrganizationId == organizationId && p.IsActive && p.Status == "Active")
                .ToListAsync();
        }

        public async Task UpdateCompletionPercentageAsync(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project != null && project.Tasks.Any())
            {
                var completedTasks = project.Tasks.Count(t => t.CompletionPercentage >= 100);
                var totalTasks = project.Tasks.Count;
                
                project.CompletionPercentage = (decimal)completedTasks / totalTasks * 100;
                project.UpdatedAt = DateTime.UtcNow;
                
                _context.Projects.Update(project);
            }
        }
    }
}
