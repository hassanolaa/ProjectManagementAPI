namespace TaskManagementAPI.Models.DTOs.Task
{
    public class TaskAttachmentDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string UploadedByUserId { get; set; } = string.Empty;
        public string UploadedByUserName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string? ContentType { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
