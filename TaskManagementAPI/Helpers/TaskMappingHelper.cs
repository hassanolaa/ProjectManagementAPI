using TaskManagementAPI.Models.DTOs.Task;
using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Helpers
{
    public static class TaskMappingHelper
    {
        public static TaskDto MapToDto(TaskItem task)
        {
            return new TaskDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name ?? "",
                TaskStatusId = task.TaskStatusId,
                TaskStatusName = task.TaskStatus?.Name ?? "",
                TaskStatusColor = task.TaskStatus?.Color,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}" : null,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUserName = $"{task.CreatedByUser?.FirstName} {task.CreatedByUser?.LastName}",
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours,
                CompletionPercentage = task.CompletionPercentage,
                StartDate = task.StartDate,
                CompletedDate = task.CompletedDate,
                Tags = task.Tags,
                IsActive = task.IsActive,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                IsOverdue = task.DueDate.HasValue && task.DueDate.Value < DateTime.UtcNow && task.CompletionPercentage < 100
            };
        }

        public static TaskDetailsDto MapToDetailsDto(TaskItem task)
        {
            var dto = new TaskDetailsDto
            {
                Id = task.Id,
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name ?? "",
                TaskStatusId = task.TaskStatusId,
                TaskStatusName = task.TaskStatus?.Name ?? "",
                TaskStatusColor = task.TaskStatus?.Color,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToUserName = task.AssignedToUser != null ? $"{task.AssignedToUser.FirstName} {task.AssignedToUser.LastName}" : null,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUserName = $"{task.CreatedByUser?.FirstName} {task.CreatedByUser?.LastName}",
                Title = task.Title,
                Description = task.Description,
                Priority = task.Priority,
                DueDate = task.DueDate,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours,
                CompletionPercentage = task.CompletionPercentage,
                StartDate = task.StartDate,
                CompletedDate = task.CompletedDate,
                Tags = task.Tags,
                IsActive = task.IsActive,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                IsOverdue = task.DueDate.HasValue && task.DueDate.Value < DateTime.UtcNow && task.CompletionPercentage < 100,
                Comments = task.Comments?.Select(MapCommentToDto).ToList() ?? new(),
                Attachments = task.Attachments?.Select(MapAttachmentToDto).ToList() ?? new(),
                TimeEntries = task.TimeEntries?.Select(MapTimeEntryToDto).ToList() ?? new()
            };

            return dto;
        }

        private static TaskCommentDto MapCommentToDto(TaskComment comment)
        {
            return new TaskCommentDto
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = $"{comment.User?.FirstName} {comment.User?.LastName}",
                ParentCommentId = comment.ParentCommentId,
                Content = comment.Content,
                IsEdited = comment.IsEdited,
                EditedAt = comment.EditedAt,
                CreatedAt = comment.CreatedAt,
                Replies = comment.Replies?.Select(MapCommentToDto).ToList() ?? new()
            };
        }

        private static TaskAttachmentDto MapAttachmentToDto(TaskAttachment attachment)
        {
            return new TaskAttachmentDto
            {
                Id = attachment.Id,
                TaskId = attachment.TaskId,
                UploadedByUserId = attachment.UploadedByUserId,
                UploadedByUserName = $"{attachment.UploadedByUser?.FirstName} {attachment.UploadedByUser?.LastName}",
                FileName = attachment.FileName,
                FilePath = attachment.FilePath,
                FileSize = attachment.FileSize,
                ContentType = attachment.ContentType,
                Description = attachment.Description,
                CreatedAt = attachment.CreatedAt
            };
        }

        private static TimeEntryDto MapTimeEntryToDto(TimeEntry timeEntry)
        {
            return new TimeEntryDto
            {
                Id = timeEntry.Id,
                TaskId = timeEntry.TaskId,
                UserId = timeEntry.UserId,
                UserName = $"{timeEntry.User?.FirstName} {timeEntry.User?.LastName}",
                Date = timeEntry.Date,
                StartTime = timeEntry.StartTime,
                EndTime = timeEntry.EndTime,
                Hours = timeEntry.Hours,
                Description = timeEntry.Description,
                IsBillable = timeEntry.IsBillable,
                CreatedAt = timeEntry.CreatedAt
            };
        }
    }
}
