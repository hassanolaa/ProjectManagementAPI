

using TaskManagementAPI.Models.DTOs.TaskStatus;
using TaskManagementAPI.Repository.Interfaces;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Services.Implementations
{
    public class TaskStatusService : ITaskStatusService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TaskStatusService> _logger;

        public TaskStatusService(IUnitOfWork unitOfWork, ILogger<TaskStatusService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public Task<TaskStatusDto> CreateAsync(CreateTaskStatusDto dto, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<TaskStatusDto?> GetByIdAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TaskStatusDto>> GetByProjectAsync(int projectId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<TaskStatusDto> UpdateAsync(int id, UpdateStatusDto dto, string userId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TaskStatusDto>> ReorderStatusesAsync(int projectId, List<int> statusIds, string userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TaskStatusDto>> CreateDefaultStatusesAsync(int projectId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}