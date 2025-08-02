using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Repository.Interfaces
{
    public interface ITaskStatusRepository : IGenericRepository<Models.Entities.TaskStatus>
    {
        Task<IEnumerable<Models.Entities.TaskStatus>> GetByProjectAsync(int projectId);
        Task<Models.Entities.TaskStatus?> GetDefaultStatusAsync(int projectId);
        Task<Models.Entities.TaskStatus?> GetCompletedStatusAsync(int projectId);
        Task ReorderStatusesAsync(int projectId, List<int> statusIds);
        Task<bool> CanDeleteStatusAsync(int statusId);
    }
}
