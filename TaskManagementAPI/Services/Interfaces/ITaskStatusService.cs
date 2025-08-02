using TaskManagementAPI.Models.DTOs.TaskStatus;

namespace TaskManagementAPI.Services.Interfaces
{
    public interface ITaskStatusService
    {
        Task<TaskStatusDto> CreateAsync(CreateTaskStatusDto dto, string userId);
        Task<TaskStatusDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<TaskStatusDto>> GetByProjectAsync(int projectId, string userId);
        Task<TaskStatusDto> UpdateAsync(int id, UpdateStatusDto dto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<IEnumerable<TaskStatusDto>> ReorderStatusesAsync(int projectId, List<int> statusIds, string userId);
        Task<IEnumerable<TaskStatusDto>> CreateDefaultStatusesAsync(int projectId, string userId);
    }
}
