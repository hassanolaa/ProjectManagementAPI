namespace TaskManagementAPI.Models.DTOs.Task
{
    public class TaskDetailsDto : TaskDto
    {
        public List<TaskCommentDto> Comments { get; set; } = new();
        public List<TaskAttachmentDto> Attachments { get; set; } = new();
        public List<TimeEntryDto> TimeEntries { get; set; } = new();
    }
}
