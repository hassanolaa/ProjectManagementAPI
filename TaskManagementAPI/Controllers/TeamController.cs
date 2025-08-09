using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TaskManagementAPI.Models.DTOs.Team;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TeamController : ControllerBase
    {
        private readonly ITeamService _teamService;
        private readonly ILogger<TeamController> _logger;

        public TeamController(
            ITeamService teamService,
            ILogger<TeamController> logger)
        {
            _teamService = teamService;
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
        /// Create a new team
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamService.CreateAsync(dto, userId);
                
                _logger.LogInformation("Team created successfully by user {UserId}", userId);
                return CreatedAtAction(nameof(GetTeam), new { id = result.Id }, result);
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
                _logger.LogError(ex, "Error creating team");
                return StatusCode(500, new { message = "An error occurred during team creation" });
            }
        }

        /// <summary>
        /// Get team by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDto>> GetTeam(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamService.GetByIdAsync(id, userId);
                
                if (result == null)
                {
                    return NotFound(new { message = "Team not found or access denied" });
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
                _logger.LogError(ex, "Error retrieving team {TeamId}", id);
                return StatusCode(500, new { message = "An error occurred during team retrieval" });
            }
        }

        /// <summary>
        /// Get all teams for current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetUserTeams()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamService.GetUserTeamsAsync(userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user teams");
                return StatusCode(500, new { message = "An error occurred during team retrieval" });
            }
        }

        /// <summary>
        /// Get teams by organization
        /// </summary>
        [HttpGet("organization/{organizationId}")]
        public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeamsByOrganization(int organizationId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamService.GetByOrganizationAsync(organizationId, userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teams for organization {OrganizationId}", organizationId);
                return StatusCode(500, new { message = "An error occurred during team retrieval" });
            }
        }

        /// <summary>
        /// Update team
        /// </summary>
        [HttpPut("{id}")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<TeamDto>> UpdateTeam(int id, [FromBody] UpdateTeamDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamService.UpdateAsync(id, dto, userId);
                
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
                _logger.LogError(ex, "Error updating team {TeamId}", id);
                return StatusCode(500, new { message = "An error occurred during team update" });
            }
        }

        /// <summary>
        /// Delete team (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeam(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _teamService.DeleteAsync(id, userId);
                
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
                _logger.LogError(ex, "Error deleting team {TeamId}", id);
                return StatusCode(500, new { message = "An error occurred during team deletion" });
            }
        }

        /// <summary>
        /// Add member to team
        /// </summary>
        [HttpPost("{id}/members")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<TeamMemberDto>> AddMember(int id, [FromBody] AddTeamMemberDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamService.AddMemberAsync(id, dto, userId);
                
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
                _logger.LogError(ex, "Error adding member to team {TeamId}", id);
                return StatusCode(500, new { message = "An error occurred during member addition" });
            }
        }

        /// <summary>
        /// Remove member from team
        /// </summary>
        [HttpDelete("{id}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(int id, string memberId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _teamService.RemoveMemberAsync(id, memberId, userId);
                
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
                _logger.LogError(ex, "Error removing member from team {TeamId}", id);
                return StatusCode(500, new { message = "An error occurred during member removal" });
            }
        }

        /// <summary>
        /// Get team members
        /// </summary>
        [HttpGet("{id}/members")]
        public async Task<ActionResult<IEnumerable<TeamMemberDto>>> GetMembers(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _teamService.GetMembersAsync(id, userId);
                
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
                _logger.LogError(ex, "Error retrieving team members {TeamId}", id);
                return StatusCode(500, new { message = "An error occurred during member retrieval" });
            }
        }

     

      
    }
}
