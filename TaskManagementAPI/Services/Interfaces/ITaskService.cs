using TaskManagementAPI.Models.DTOs.Task;

namespace TaskManagementAPI.Services.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> CreateAsync(CreateTaskDto dto, string userId);
        Task<TaskDto?> GetByIdAsync(int id, string userId);
        Task<TaskDetailsDto?> GetDetailsAsync(int id, string userId);
        Task<IEnumerable<TaskDto>> GetByProjectAsync(int projectId, string userId);
        Task<IEnumerable<TaskDto>> GetAssignedTasksAsync(string userId);
        Task<IEnumerable<TaskDto>> GetCreatedTasksAsync(string userId);
        Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<TaskDto> AssignTaskAsync(int taskId, string assigneeId, string userId);
        Task<TaskDto> UpdateStatusAsync(int taskId, int statusId, string userId);
        Task<TaskDto> UpdateProgressAsync(int taskId, decimal completionPercentage, string userId);
        Task<IEnumerable<TaskDto>> SearchTasksAsync(string searchTerm, int? projectId, string userId);
        Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(string userId);
        Task<IEnumerable<TaskDto>> GetDueTodayAsync(string userId);
        Task<IEnumerable<TaskDto>> GetUpcomingTasksAsync(string userId, int days = 7);
        Task<DashboardStatsDto> GetDashboardStatsAsync(string userId);
    }
}
