

namespace TaskManagementAPI.Repository.Implementations
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore;
    using TaskManagementAPI.Data;
    using TaskManagementAPI.Models.Entities;
    using TaskManagementAPI.Repository.Interfaces;

    public class TaskStatusRepository : GenericRepository<TaskStatus>, ITaskStatusRepository
    {
        public TaskStatusRepository(ApplicationDbContext context) : base(context)
        {

        }

        public Task<bool> CanDeleteStatusAsync(int statusId)
        {
            return _context.Tasks.AnyAsync(t => t.TaskStatusId == statusId && t.IsActive);
        }

        public Task<IEnumerable<TaskStatus>> GetByProjectAsync(int projectId)
        {
            throw new NotImplementedException("This method needs to be implemented.");
        }

        public Task<TaskStatus?> GetCompletedStatusAsync(int projectId)
        {
            return _context.TaskStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(ts => ts.ProjectId == projectId  && ts.IsCompleted);
        
        }

        public Task<TaskStatus?> GetDefaultStatusAsync(int projectId)
        {
            return _context.TaskStatuses
                .AsNoTracking()
                .FirstOrDefaultAsync(ts => ts.ProjectId == projectId  && ts.IsDefault);
        }

        public Task ReorderStatusesAsync(int projectId, List<int> statusIds)
        {
            throw new NotImplementedException();
        }
    }

}