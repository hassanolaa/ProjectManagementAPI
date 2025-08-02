using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TaskManagementAPI.Models.DTOs.Task;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly ILogger<TaskController> _logger;

        public TaskController(
            ITaskService taskService,
            ILogger<TaskController> logger)
        {
            _taskService = taskService;
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
        /// Create a new task
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<TaskDto>> CreateTask([FromBody] CreateTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.CreateAsync(dto, userId);
                
                _logger.LogInformation("Task created successfully by user {UserId}", userId);
                return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating task");
                return StatusCode(500, new { message = "An error occurred during task creation" });
            }
        }

        /// <summary>
        /// Get task by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetByIdAsync(id, userId);
                
                if (result == null)
                {
                    return NotFound(new { message = "Task not found or access denied" });
                }
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred during task retrieval" });
            }
        }

        /// <summary>
        /// Get task details with comments, attachments, and time entries
        /// </summary>
        [HttpGet("{id}/details")]
        public async Task<ActionResult<TaskDetailsDto>> GetTaskDetails(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetDetailsAsync(id, userId);
                
                if (result == null)
                {
                    return NotFound(new { message = "Task not found or access denied" });
                }
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task details {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred during task details retrieval" });
            }
        }

        /// <summary>
        /// Get tasks by project
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasksByProject(int projectId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetByProjectAsync(projectId, userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "An error occurred during task retrieval" });
            }
        }

        /// <summary>
        /// Get tasks assigned to current user
        /// </summary>
        [HttpGet("assigned")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetAssignedTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetAssignedTasksAsync(userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assigned tasks");
                return StatusCode(500, new { message = "An error occurred during assigned tasks retrieval" });
            }
        }

        /// <summary>
        /// Get tasks created by current user
        /// </summary>
        [HttpGet("created")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetCreatedTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetCreatedTasksAsync(userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving created tasks");
                return StatusCode(500, new { message = "An error occurred during created tasks retrieval" });
            }
        }

        /// <summary>
        /// Get overdue tasks for current user
        /// </summary>
        [HttpGet("overdue")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetOverdueTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetOverdueTasksAsync(userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving overdue tasks");
                return StatusCode(500, new { message = "An error occurred during overdue tasks retrieval" });
            }
        }

        /// <summary>
        /// Get tasks due today for current user
        /// </summary>
        [HttpGet("due-today")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetDueTodayTasks()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetDueTodayAsync(userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving due today tasks");
                return StatusCode(500, new { message = "An error occurred during due today tasks retrieval" });
            }
        }

        /// <summary>
        /// Get upcoming tasks for current user
        /// </summary>
        [HttpGet("upcoming")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetUpcomingTasks([FromQuery] int days = 7)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.GetUpcomingTasksAsync(userId, days);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving upcoming tasks");
                return StatusCode(500, new { message = "An error occurred during upcoming tasks retrieval" });
            }
        }

        /// <summary>
        /// Search tasks
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> SearchTasks(
            [FromQuery] string searchTerm, 
            [FromQuery] int? projectId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest(new { message = "Search term is required" });
                }

                var userId = GetCurrentUserId();
                var result = await _taskService.SearchTasksAsync(searchTerm, projectId, userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tasks");
                return StatusCode(500, new { message = "An error occurred during task search" });
            }
        }

        /// <summary>
        /// Update task
        /// </summary>
        [HttpPut("{id}")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<TaskDto>> UpdateTask(int id, [FromBody] UpdateTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.UpdateAsync(id, dto, userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred during task update" });
            }
        }

        /// <summary>
        /// Delete task (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _taskService.DeleteAsync(id, userId);
                
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred during task deletion" });
            }
        }

        /// <summary>
        /// Assign task to user
        /// </summary>
        [HttpPut("{id}/assign")]
        public async Task<ActionResult<TaskDto>> AssignTask(int id, [FromBody] AssignTaskDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.AssignTaskAsync(id, dto.AssigneeId, userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning task {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred during task assignment" });
            }
        }

        /// <summary>
        /// Update task status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<TaskDto>> UpdateTaskStatus(int id, [FromBody] UpdateTaskStatusDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.UpdateStatusAsync(id, dto.StatusId, userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task status {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred during status update" });
            }
        }

        /// <summary>
        /// Update task progress
        /// </summary>
        [HttpPut("{id}/progress")]
        public async Task<ActionResult<TaskDto>> UpdateTaskProgress(int id, [FromBody] UpdateTaskProgressDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskService.UpdateProgressAsync(id, dto.CompletionPercentage, userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task progress {TaskId}", id);
                return StatusCode(500, new { message = "An error occurred during progress update" });
            }
        }
    }
}
