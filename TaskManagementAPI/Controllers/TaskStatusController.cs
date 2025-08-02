using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TaskManagementAPI.Models.DTOs.TaskStatus;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaskStatusController : ControllerBase
    {
        private readonly ITaskStatusService _taskStatusService;
        private readonly ILogger<TaskStatusController> _logger;

        public TaskStatusController(
            ITaskStatusService taskStatusService,
            ILogger<TaskStatusController> logger)
        {
            _taskStatusService = taskStatusService;
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
        /// Get task statuses for a project
        /// </summary>
        [HttpGet("project/{projectId}")]
        public async Task<ActionResult<IEnumerable<TaskStatusDto>>> GetTaskStatuses(int projectId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskStatusService.GetByProjectAsync(projectId, userId);
                
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
                _logger.LogError(ex, "Error retrieving task statuses for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "An error occurred during task status retrieval" });
            }
        }

        /// <summary>
        /// Create a new task status
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<TaskStatusDto>> CreateTaskStatus([FromBody] CreateTaskStatusDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskStatusService.CreateAsync(dto, userId);
                
                return CreatedAtAction(nameof(GetTaskStatus), new { id = result.Id }, result);
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
                _logger.LogError(ex, "Error creating task status");
                return StatusCode(500, new { message = "An error occurred during task status creation" });
            }
        }

        /// <summary>
        /// Get task status by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskStatusDto>> GetTaskStatus(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskStatusService.GetByIdAsync(id, userId);
                
                if (result == null)
                {
                    return NotFound(new { message = "Task status not found or access denied" });
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
                _logger.LogError(ex, "Error retrieving task status {StatusId}", id);
                return StatusCode(500, new { message = "An error occurred during task status retrieval" });
            }
        }

        /// <summary>
        /// Update task status
        /// </summary>
        [HttpPut("{id}")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<TaskStatusDto>> UpdateTaskStatus(int id, [FromBody] UpdateStatusDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _taskStatusService.UpdateAsync(id, dto, userId);
                
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
                _logger.LogError(ex, "Error updating task status {StatusId}", id);
                return StatusCode(500, new { message = "An error occurred during task status update" });
            }
        }

        /// <summary>
        /// Delete task status
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTaskStatus(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _taskStatusService.DeleteAsync(id, userId);
                
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task status {StatusId}", id);
                return StatusCode(500, new { message = "An error occurred during task status deletion" });
            }
        }

        /// <summary>
        /// Reorder task statuses for a project
        /// </summary>
        [HttpPut("project/{projectId}/reorder")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<IEnumerable<TaskStatusDto>>> ReorderStatuses(int projectId, [FromBody] List<int> statusIds)
        {
            try
            {
                if (statusIds == null || !statusIds.Any())
                {
                    return BadRequest(new { message = "Status IDs list is required and cannot be empty" });
                }

                var userId = GetCurrentUserId();
                var result = await _taskStatusService.ReorderStatusesAsync(projectId, statusIds, userId);
                
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering task statuses for project {ProjectId}", projectId);
                return StatusCode(500, new { message = "An error occurred during status reordering" });
            }
        }
    }
}
