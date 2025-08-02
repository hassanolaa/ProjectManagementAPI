namespace TaskManagementAPI.Models.DTOs.Project
{
    public class ProjectStatsDto
    {
        public int ProjectId { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public decimal CompletionPercentage { get; set; }
    }
}
