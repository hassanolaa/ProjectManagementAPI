using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TaskManagementAPI.Models.DTOs.Project;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(
            IProjectService projectService,
            ILogger<ProjectController> logger)
        {
            _projectService = projectService;
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
        /// Create a new project
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<ProjectDto>> CreateProject([FromBody] CreateProjectDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.CreateAsync(dto, userId);
                
                _logger.LogInformation("Project created successfully by user {UserId}", userId);
                return CreatedAtAction(nameof(GetProject), new { id = result.Id }, result);
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
                _logger.LogError(ex, "Error creating project");
                return StatusCode(500, new { message = "An error occurred during project creation" });
            }
        }

        /// <summary>
        /// Get project by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProjectDto>> GetProject(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.GetByIdAsync(id, userId);
                
                if (result == null)
                {
                    return NotFound(new { message = "Project not found or access denied" });
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
                _logger.LogError(ex, "Error retrieving project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred during project retrieval" });
            }
        }

        /// <summary>
        /// Get all projects for current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetUserProjects()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.GetUserProjectsAsync(userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user projects");
                return StatusCode(500, new { message = "An error occurred during project retrieval" });
            }
        }

        /// <summary>
        /// Get projects by organization
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjectsByOrganization(int organizationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.GetByOrganizationAsync(organizationId, userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving projects for organization {OrganizationId}", organizationId);
                return StatusCode(500, new { message = "An error occurred during project retrieval" });
            }
        }

        /// <summary>
        /// Update project
        /// </summary>
        [HttpPut("{id}")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<ProjectDto>> UpdateProject(int id, [FromBody] UpdateProjectDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.UpdateAsync(id, dto, userId);
                
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
                _logger.LogError(ex, "Error updating project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred during project update" });
            }
        }

        /// <summary>
        /// Delete project (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _projectService.DeleteAsync(id, userId);
                
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
                _logger.LogError(ex, "Error deleting project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred during project deletion" });
            }
        }

        /// <summary>
        /// Update project status
        /// </summary>
        [HttpPut("{id}/status")]
        public async Task<ActionResult<ProjectDto>> UpdateProjectStatus(int id, [FromBody] UpdateProjectStatusDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.UpdateStatusAsync(id, dto.Status, userId);
                
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
                _logger.LogError(ex, "Error updating project status {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred during status update" });
            }
        }

        /// <summary>
        /// Get project statistics
        /// </summary>
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<ProjectStatsDto>> GetProjectStats(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.GetProjectStatsAsync(id, userId);
                
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
                _logger.LogError(ex, "Error retrieving project stats {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred during stats retrieval" });
            }
        }

        /// <summary>
        /// Add member to project
        /// </summary>
        [HttpPost("{id}/members")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<ProjectMemberDto>> AddMember(int id, [FromBody] AddProjectMemberDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.AddMemberAsync(id, dto, userId);
                
                return CreatedAtAction(nameof(GetMembers), new { id = id }, result);
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
                _logger.LogError(ex, "Error adding member to project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred during member addition" });
            }
        }

        /// <summary>
        /// Remove member from project
        /// </summary>
        [HttpDelete("{id}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(int id, string memberId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _projectService.RemoveMemberAsync(id, memberId, userId);
                
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
                _logger.LogError(ex, "Error removing member from project {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred during member removal" });
            }
        }

        /// <summary>
        /// Get project members
        /// </summary>
        [HttpGet("{id}/members")]
        public async Task<ActionResult<IEnumerable<ProjectMemberDto>>> GetMembers(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _projectService.GetMembersAsync(id, userId);
                
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
                _logger.LogError(ex, "Error retrieving project members {ProjectId}", id);
                return StatusCode(500, new { message = "An error occurred during member retrieval" });
            }
        }
    }
}
