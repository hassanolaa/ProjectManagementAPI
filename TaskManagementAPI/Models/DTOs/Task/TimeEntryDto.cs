namespace TaskManagementAPI.Models.DTOs.Task
{
    public class TimeEntryDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal Hours { get; set; }
        public string? Description { get; set; }
        public bool IsBillable { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
