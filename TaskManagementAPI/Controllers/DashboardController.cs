using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagementAPI.Models.DTOs.Task;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            ITaskService taskService,
            IProjectService projectService,
            IOrganizationService organizationService,
            ILogger<DashboardController> logger)
        {
            _taskService = taskService;
            _projectService = projectService;
            _organizationService = organizationService;
            _logger = logger;
        }

        private string GetCurrentUserId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                       ?? User.FindFirst("sub")?.Value 
                       ?? User.FindFirst("id")?.Value;
            
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            
            return userId;
        }

        /// <summary>
        /// Get dashboard statistics for current user
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetDashboardStatsAsync(userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return StatusCode(500, new { message = "An error occurred during dashboard stats retrieval" });
            }
        }

        /// <summary>
        /// Get dashboard overview including tasks, projects, and organizations
        /// </summary>
        [HttpGet("overview")]
        public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview()
        {
            try
            {
                var userId = GetCurrentUserId();

                // Get all data in parallel
                var statsTask = _taskService.GetDashboardStatsAsync(userId);
                var recentTasksTask = _taskService.GetAssignedTasksAsync(userId);
                var overdueTasksTask = _taskService.GetOverdueTasksAsync(userId);
                var dueTodayTasksTask = _taskService.GetDueTodayAsync(userId);
                var projectsTask = _projectService.GetUserProjectsAsync(userId);
                var organizationsTask = _organizationService.GetUserOrganizationsAsync(userId);

                await Task.WhenAll(statsTask, recentTasksTask, overdueTasksTask, 
                    dueTodayTasksTask, projectsTask, organizationsTask);

                var overview = new DashboardOverviewDto
                {
                    Stats = await statsTask,
                    RecentTasks = (await recentTasksTask).Take(5).ToList(),
                    OverdueTasks = (await overdueTasksTask).Take(5).ToList(),
                    DueTodayTasks = (await dueTodayTasksTask).ToList(),
                    RecentProjects = (await projectsTask).Take(5).ToList(),
                    Organizations = (await organizationsTask).ToList()
                };

                return Ok(overview);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard overview");
                return StatusCode(500, new { message = "An error occurred during dashboard overview retrieval" });
            }
        }

        /// <summary>
        /// Get task activity feed for current user
        /// </summary>
        [HttpGet("activity")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTaskActivity([FromQuery] int days = 7)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Get recent activity (combination of assigned and created tasks)
                var assignedTasks = await _taskService.GetAssignedTasksAsync(userId);
                var createdTasks = await _taskService.GetCreatedTasksAsync(userId);

                var allTasks = assignedTasks.Concat(createdTasks)
                    .Where(t => t.UpdatedAt >= DateTime.UtcNow.AddDays(-days))
                    .OrderByDescending(t => t.UpdatedAt)
                    .Take(20)
                    .ToList();

                return Ok(allTasks);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task activity");
                return StatusCode(500, new { message = "An error occurred during activity retrieval" });
            }
        }
    }
}
