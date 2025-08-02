using TaskManagementAPI.Models.DTOs.Organization;
using TaskManagementAPI.Models.DTOs.Project;

namespace TaskManagementAPI.Models.DTOs.Task
{
    public class DashboardOverviewDto
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public List<TaskDto> RecentTasks { get; set; } = new();
        public List<TaskDto> OverdueTasks { get; set; } = new();
        public List<TaskDto> DueTodayTasks { get; set; } = new();
        public List<ProjectDto> RecentProjects { get; set; } = new();
        public List<OrganizationDto> Organizations { get; set; } = new();
    }
}
