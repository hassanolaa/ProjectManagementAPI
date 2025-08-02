namespace TaskManagementAPI.Models.DTOs.Task
{
    public class DashboardStatsDto
    {
        public int TotalAssignedTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int DueTodayTasks { get; set; }
    }
}
