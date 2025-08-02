using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Repository.Interfaces
{
    public interface ITaskRepository : IGenericRepository<TaskItem>
    {
        Task<IEnumerable<TaskItem>> GetByProjectAsync(int projectId);
        Task<IEnumerable<TaskItem>> GetByAssigneeAsync(string userId);
        Task<IEnumerable<TaskItem>> GetByCreatorAsync(string userId);
        Task<IEnumerable<TaskItem>> GetByStatusAsync(int statusId);
        Task<IEnumerable<TaskItem>> GetOverdueTasksAsync();
        Task<IEnumerable<TaskItem>> GetDueTodayAsync();
        Task<IEnumerable<TaskItem>> GetUpcomingTasksAsync(int days = 7);
        Task<TaskItem?> GetWithDetailsAsync(int taskId);
        Task<IEnumerable<TaskItem>> SearchTasksAsync(string searchTerm, int? projectId = null);
        Task<IEnumerable<TaskItem>> GetTasksByPriorityAsync(string priority, int? projectId = null);
    }
}
