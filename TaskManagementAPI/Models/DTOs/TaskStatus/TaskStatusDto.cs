namespace TaskManagementAPI.Models.DTOs.TaskStatus
{
    public class TaskStatusDto
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Order { get; set; }
        public bool IsDefault { get; set; }
        public bool IsCompleted { get; set; }
        public int TaskCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
