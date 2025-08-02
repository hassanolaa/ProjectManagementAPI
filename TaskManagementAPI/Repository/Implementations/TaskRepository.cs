using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Repository.Interfaces;

namespace TaskManagementAPI.Repository.Implementations
{
    public class TaskRepository : GenericRepository<TaskItem>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<TaskItem>> GetByProjectAsync(int projectId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.ProjectId == projectId && t.IsActive)
                .Include(t => t.TaskStatus)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .OrderBy(t => t.TaskStatus.Order)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetByAssigneeAsync(string userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.AssignedToUserId == userId && t.IsActive)
                .Include(t => t.Project)
                .Include(t => t.TaskStatus)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetByCreatorAsync(string userId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.CreatedByUserId == userId && t.IsActive)
                .Include(t => t.Project)
                .Include(t => t.TaskStatus)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetByStatusAsync(int statusId)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.TaskStatusId == statusId && t.IsActive)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Project)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetOverdueTasksAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date < today && 
                           t.CompletionPercentage < 100 && t.IsActive)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.TaskStatus)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetDueTodayAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == today && 
                           t.CompletionPercentage < 100 && t.IsActive)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.TaskStatus)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetUpcomingTasksAsync(int days = 7)
        {
            var today = DateTime.UtcNow.Date;
            var futureDate = today.AddDays(days);
            
            return await _context.Tasks
                .AsNoTracking()
                .Where(t => t.DueDate.HasValue && 
                           t.DueDate.Value.Date >= today && 
                           t.DueDate.Value.Date <= futureDate &&
                           t.CompletionPercentage < 100 && t.IsActive)
                .Include(t => t.Project)
                .Include(t => t.AssignedToUser)
                .Include(t => t.TaskStatus)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }

        public async Task<TaskItem?> GetWithDetailsAsync(int taskId)
        {
            return await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.TaskStatus)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .Include(t => t.Attachments)
                    .ThenInclude(a => a.UploadedByUser)
                .Include(t => t.TimeEntries)
                    .ThenInclude(te => te.User)
                .FirstOrDefaultAsync(t => t.Id == taskId);
        }

        public async Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm, int? projectId = null)
        {
            var query = _context.Tasks.AsNoTracking().Where(t => t.IsActive);

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            query = query.Where(t => t.Title.Contains(searchTerm) || 
                                   (t.Description != null && t.Description.Contains(searchTerm)));

            return await query
                .Include(t => t.Project)
                .Include(t => t.TaskStatus)
                .Include(t => t.AssignedToUser)
                .OrderByDescending(t => t.UpdatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(string priority, int? projectId = null)
        {
            var query = _context.Tasks.AsNoTracking().Where(t => t.Priority == priority && t.IsActive);

            if (projectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == projectId.Value);
            }

            return await query
                .Include(t => t.Project)
                .Include(t => t.TaskStatus)
                .Include(t => t.AssignedToUser)
                .OrderBy(t => t.DueDate)
                .ToListAsync();
        }
    }
}
