using TaskManagementAPI.Models.DTOs.TaskStatus;
using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Helpers
{
    public static class TaskStatusMappingHelper
    {
        public static TaskStatusDto MapToDto(Models.Entities.TaskStatus taskStatus, int taskCount = 0)
        {
            return new TaskStatusDto
            {
                Id = taskStatus.Id,
                ProjectId = taskStatus.ProjectId,
                Name = taskStatus.Name,
                Color = taskStatus.Color,
                Order = taskStatus.Order,
                IsDefault = taskStatus.IsDefault,
                IsCompleted = taskStatus.IsCompleted,
                TaskCount = taskCount,
                CreatedAt = taskStatus.CreatedAt
            };
        }

        public static List<TaskStatusDto> MapToDto(IEnumerable<Models.Entities.TaskStatus> taskStatuses)
        {
            return taskStatuses.Select(ts => MapToDto(ts)).ToList();
        }

        public static Models.Entities.TaskStatus MapToEntity(CreateTaskStatusDto dto)
        {
            return new Models.Entities.TaskStatus
            {
                ProjectId = dto.ProjectId,
                Name = dto.Name,
                Color = dto.Color,
                Order = dto.Order,
                IsDefault = dto.IsDefault,
                IsCompleted = dto.IsCompleted,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void UpdateEntity(Models.Entities.TaskStatus entity, UpdateStatusDto dto)
        {
            if (!string.IsNullOrEmpty(dto.Name))
                entity.Name = dto.Name;
            
            if (!string.IsNullOrEmpty(dto.Color))
                entity.Color = dto.Color;
            
            if (dto.Order.HasValue)
                entity.Order = dto.Order.Value;
            
            if (dto.IsDefault.HasValue)
                entity.IsDefault = dto.IsDefault.Value;
            
            if (dto.IsCompleted.HasValue)
                entity.IsCompleted = dto.IsCompleted.Value;
            
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}
