using TaskManagementAPI.Models.DTOs.Task;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Repository.Interfaces;
using TaskManagementAPI.Services.Interfaces;
using TaskManagementAPI.Helpers;

namespace TaskManagementAPI.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TaskService> _logger;

        public TaskService(IUnitOfWork unitOfWork, ILogger<TaskService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<TaskDto> CreateAsync(CreateTaskDto dto, string userId)
        {
            // Check if user has access to the project
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(dto.ProjectId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this project");

            // Get default status if not provided
            int statusId = dto.TaskStatusId;
            if (statusId == 0)
            {
                var defaultStatus = await _unitOfWork.TaskStatuses.GetDefaultStatusAsync(dto.ProjectId);
                if (defaultStatus == null)
                    throw new InvalidOperationException("No default status found for this project");
                statusId = defaultStatus.Id;
            }

            var task = new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                ProjectId = dto.ProjectId,
                TaskStatusId = statusId,
                AssignedToUserId = dto.AssignedToUserId,
                CreatedByUserId = userId,
                Priority = dto.Priority ?? "Medium",
                DueDate = dto.DueDate,
                EstimatedHours = dto.EstimatedHours,
                CompletionPercentage = 0,
                Tags = dto.Tags,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Tasks.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} created by user {UserId}", task.Id, userId);

            return TaskMappingHelper.MapToDto(task);
        }

        public async Task<TaskDto?> GetByIdAsync(int id, string userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null) return null;

            // Check if user has access to the project
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
            if (!isMember) return null;

            return TaskMappingHelper.MapToDto(task);
        }

        public async Task<TaskDetailsDto?> GetDetailsAsync(int id, string userId)
        {
            var task = await _unitOfWork.Tasks.GetWithDetailsAsync(id);
            if (task == null) return null;

            // Check if user has access to the project
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
            if (!isMember) return null;

            return TaskMappingHelper.MapToDetailsDto(task);
        }

        public async Task<IEnumerable<TaskDto>> GetByProjectAsync(int projectId, string userId)
        {
            // Check if user has access to the project
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(projectId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this project");

            var tasks = await _unitOfWork.Tasks.GetByProjectAsync(projectId);
            return tasks.Select(TaskMappingHelper.MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetAssignedTasksAsync(string userId)
        {
            var tasks = await _unitOfWork.Tasks.GetByAssigneeAsync(userId);
            return tasks.Select(TaskMappingHelper.MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetCreatedTasksAsync(string userId)
        {
            var tasks = await _unitOfWork.Tasks.GetByCreatorAsync(userId);
            return tasks.Select(TaskMappingHelper.MapToDto);
        }

        public async Task<TaskDto> UpdateAsync(int id, UpdateTaskDto dto, string userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            // Check permissions - user must be project member
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this task");

            // Update fields if provided
            if (!string.IsNullOrEmpty(dto.Title))
                task.Title = dto.Title;

            if (dto.Description != null)
                task.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Priority))
                task.Priority = dto.Priority;

            if (dto.DueDate != null)
                task.DueDate = dto.DueDate;

            if (dto.EstimatedHours != null)
                task.EstimatedHours = dto.EstimatedHours;

            if (dto.CompletionPercentage.HasValue)
            {
                task.CompletionPercentage = dto.CompletionPercentage.Value;
                
                // Set completed date if task is completed
                if (dto.CompletionPercentage.Value >= 100 && task.CompletedDate == null)
                {
                    task.CompletedDate = DateTime.UtcNow;
                }
                else if (dto.CompletionPercentage.Value < 100)
                {
                    task.CompletedDate = null;
                }
            }

            if (dto.Tags != null)
                task.Tags = dto.Tags;

            task.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task);
            
            // Update project completion percentage
            await _unitOfWork.Projects.UpdateCompletionPercentageAsync(task.ProjectId);
            
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} updated by user {UserId}", id, userId);

            return TaskMappingHelper.MapToDto(task);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(id);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            // Check permissions - user must be project member or task creator
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
            var isCreator = task.CreatedByUserId == userId;
            
            if (!isMember && !isCreator)
                throw new UnauthorizedAccessException("You don't have permission to delete this task");

            // Soft delete - set IsActive to false
            task.IsActive = false;
            task.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task);
            
            // Update project completion percentage
            await _unitOfWork.Projects.UpdateCompletionPercentageAsync(task.ProjectId);
            
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} deleted by user {UserId}", id, userId);
        }

        public async Task<TaskDto> AssignTaskAsync(int taskId, string assigneeId, string userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            // Check permissions - user must be project member
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this task");

            // Verify assignee has access to the project
            if (!string.IsNullOrEmpty(assigneeId))
            {
                var assigneeIsMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, assigneeId);
                if (!assigneeIsMember)
                    throw new InvalidOperationException("Assignee does not have access to this project");
            }

            task.AssignedToUserId = assigneeId;
            task.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Tasks.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} assigned to user {AssigneeId} by {UserId}", 
                taskId, assigneeId ?? "unassigned", userId);

            return TaskMappingHelper.MapToDto(task);
        }

        public async Task<TaskDto> UpdateProgressAsync(int taskId, decimal completionPercentage, string userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            // Check permissions - user must be assigned to task or be a project member
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
            var isAssignee = task.AssignedToUserId == userId;
            
            if (!isMember && !isAssignee)
                throw new UnauthorizedAccessException("You don't have permission to update this task progress");

            // Validate completion percentage
            if (completionPercentage < 0 || completionPercentage > 100)
                throw new ArgumentException("Completion percentage must be between 0 and 100");

            task.CompletionPercentage = completionPercentage;
            task.UpdatedAt = DateTime.UtcNow;

            // Set completed date if task is completed
            if (completionPercentage >= 100 && task.CompletedDate == null)
            {
                task.CompletedDate = DateTime.UtcNow;
            }
            else if (completionPercentage < 100)
            {
                task.CompletedDate = null;
            }

            await _unitOfWork.Tasks.UpdateAsync(task);
            
            // Update project completion percentage
            await _unitOfWork.Projects.UpdateCompletionPercentageAsync(task.ProjectId);
            
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} progress updated to {Progress}% by user {UserId}", 
                taskId, completionPercentage, userId);

            return TaskMappingHelper.MapToDto(task);
        }

        public async Task<IEnumerable<TaskDto>> SearchTasksAsync(string searchTerm, int? projectId, string userId)
        {
            // If project is specified, check access
            if (projectId.HasValue)
            {
                var isMember = await _unitOfWork.Projects.IsUserMemberAsync(projectId.Value, userId);
                if (!isMember)
                    throw new UnauthorizedAccessException("You don't have access to this project");
            }

            var tasks = await _unitOfWork.Tasks.SearchTasksAsync(searchTerm, projectId);
            
            // Filter tasks to only include those in projects the user has access to
            var accessibleTasks = new List<TaskItem>();
            foreach (var task in tasks)
            {
                var hasAccess = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
                if (hasAccess)
                {
                    accessibleTasks.Add(task);
                }
            }

            return accessibleTasks.Select(TaskMappingHelper.MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync(string userId)
        {
            var overdueTasks = await _unitOfWork.Tasks.GetOverdueTasksAsync();
            
            // Filter to tasks user has access to (assigned or in accessible projects)
            var userAccessibleTasks = new List<TaskItem>();
            foreach (var task in overdueTasks)
            {
                var isAssignee = task.AssignedToUserId == userId;
                var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
                
                if (isAssignee || isMember)
                {
                    userAccessibleTasks.Add(task);
                }
            }

            return userAccessibleTasks.Select(TaskMappingHelper.MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetDueTodayAsync(string userId)
        {
            var dueTodayTasks = await _unitOfWork.Tasks.GetDueTodayAsync();
            
            // Filter to tasks user has access to (assigned or in accessible projects)
            var userAccessibleTasks = new List<TaskItem>();
            foreach (var task in dueTodayTasks)
            {
                var isAssignee = task.AssignedToUserId == userId;
                var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
                
                if (isAssignee || isMember)
                {
                    userAccessibleTasks.Add(task);
                }
            }

            return userAccessibleTasks.Select(TaskMappingHelper.MapToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetUpcomingTasksAsync(string userId, int days = 7)
        {
            var upcomingTasks = await _unitOfWork.Tasks.GetUpcomingTasksAsync(days);
            
            // Filter to tasks user has access to (assigned or in accessible projects)
            var userAccessibleTasks = new List<TaskItem>();
            foreach (var task in upcomingTasks)
            {
                var isAssignee = task.AssignedToUserId == userId;
                var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
                
                if (isAssignee || isMember)
                {
                    userAccessibleTasks.Add(task);
                }
            }

            return userAccessibleTasks.Select(TaskMappingHelper.MapToDto);
        }

        public async Task<TaskDto> UpdateStatusAsync(int taskId, int statusId, string userId)
        {
            var task = await _unitOfWork.Tasks.GetByIdAsync(taskId);
            if (task == null)
                throw new KeyNotFoundException("Task not found");

            // Check permissions
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(task.ProjectId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this task");

            // Verify status belongs to the same project
            var status = await _unitOfWork.TaskStatuses.GetByIdAsync(statusId);
            if (status == null || status.ProjectId != task.ProjectId)
                throw new InvalidOperationException("Invalid status for this task");

            task.TaskStatusId = statusId;
            task.UpdatedAt = DateTime.UtcNow;

            // If moving to completed status, set completion to 100%
            if (status.IsCompleted)
            {
                task.CompletionPercentage = 100;
                task.CompletedDate = DateTime.UtcNow;
            }

            await _unitOfWork.Tasks.UpdateAsync(task);
            
            // Update project completion percentage
            await _unitOfWork.Projects.UpdateCompletionPercentageAsync(task.ProjectId);
            
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Task {TaskId} status updated by user {UserId}", taskId, userId);

            return TaskMappingHelper.MapToDto(task);
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(string userId)
        {
            var assignedTasks = await _unitOfWork.Tasks.GetByAssigneeAsync(userId);
            var overdueTasks = await _unitOfWork.Tasks.GetOverdueTasksAsync();
            var dueTodayTasks = await _unitOfWork.Tasks.GetDueTodayAsync();

            var userOverdueTasks = overdueTasks.Where(t => t.AssignedToUserId == userId).Count();
            var userDueTodayTasks = dueTodayTasks.Where(t => t.AssignedToUserId == userId).Count();

            return new DashboardStatsDto
            {
                TotalAssignedTasks = assignedTasks.Count(),
                CompletedTasks = assignedTasks.Count(t => t.CompletionPercentage >= 100),
                OverdueTasks = userOverdueTasks,
                DueTodayTasks = userDueTodayTasks,
                InProgressTasks = assignedTasks.Count(t => t.CompletionPercentage > 0 && t.CompletionPercentage < 100)
            };
        }
    }
}
