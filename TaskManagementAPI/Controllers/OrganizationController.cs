using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TaskManagementAPI.Models.DTOs.Organization;
using TaskManagementAPI.Services.Interfaces;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly ILogger<OrganizationController> _logger;

        public OrganizationController(
            IOrganizationService organizationService,
            ILogger<OrganizationController> logger)
        {
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
        /// Create a new organization
        /// </summary>
        [HttpPost]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<OrganizationDto>> CreateOrganization([FromBody] CreateOrganizationDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _organizationService.CreateAsync(dto, userId);
                
                _logger.LogInformation("Organization created successfully by user {UserId}", userId);
                return CreatedAtAction(nameof(GetOrganization), new { id = result.Id }, result);
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
                _logger.LogError(ex, "Error creating organization");
                return StatusCode(500, new { message = "An error occurred during organization creation" });
            }
        }

        /// <summary>
        /// Get organization by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDto>> GetOrganization(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _organizationService.GetByIdAsync(id, userId);
                
                if (result == null)
                {
                    return NotFound(new { message = "Organization not found or access denied" });
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
                _logger.LogError(ex, "Error retrieving organization {OrganizationId}", id);
                return StatusCode(500, new { message = "An error occurred during organization retrieval" });
            }
        }

        /// <summary>
        /// Get all organizations for current user
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetUserOrganizations()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _organizationService.GetUserOrganizationsAsync(userId);
                
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user organizations");
                return StatusCode(500, new { message = "An error occurred during organization retrieval" });
            }
        }

        /// <summary>
        /// Update organization
        /// </summary>
        [HttpPut("{id}")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<OrganizationDto>> UpdateOrganization(int id, [FromBody] UpdateOrganizationDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _organizationService.UpdateAsync(id, dto, userId);
                
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
                _logger.LogError(ex, "Error updating organization {OrganizationId}", id);
                return StatusCode(500, new { message = "An error occurred during organization update" });
            }
        }

        /// <summary>
        /// Delete organization (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrganization(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _organizationService.DeleteAsync(id, userId);
                
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
                _logger.LogError(ex, "Error deleting organization {OrganizationId}", id);
                return StatusCode(500, new { message = "An error occurred during organization deletion" });
            }
        }

        /// <summary>
        /// Add member to organization
        /// </summary>
        [HttpPost("{id}/members")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<OrganizationMemberDto>> AddMember(int id, [FromBody] AddMemberDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _organizationService.AddMemberAsync(id, dto, userId);
                
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
                _logger.LogError(ex, "Error adding member to organization {OrganizationId}", id);
                return StatusCode(500, new { message = "An error occurred during member addition" });
            }
        }

        /// <summary>
        /// Remove member from organization
        /// </summary>
        [HttpDelete("{id}/members/{memberId}")]
        public async Task<IActionResult> RemoveMember(int id, string memberId)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _organizationService.RemoveMemberAsync(id, memberId, userId);
                
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
                _logger.LogError(ex, "Error removing member from organization {OrganizationId}", id);
                return StatusCode(500, new { message = "An error occurred during member removal" });
            }
        }

        /// <summary>
        /// Get organization members
        /// </summary>
        [HttpGet("{id}/members")]
        public async Task<ActionResult<IEnumerable<OrganizationMemberDto>>> GetMembers(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _organizationService.GetMembersAsync(id, userId);
                
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
                _logger.LogError(ex, "Error retrieving organization members {OrganizationId}", id);
                return StatusCode(500, new { message = "An error occurred during member retrieval" });
            }
        }

        /// <summary>
        /// Update member role
        /// </summary>
        [HttpPut("{id}/members/{memberId}/role")]
        public async Task<IActionResult> UpdateMemberRole(int id, string memberId, [FromBody] UpdateMemberRoleDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                await _organizationService.UpdateMemberRoleAsync(id, memberId, dto.Role, userId);
                
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
                _logger.LogError(ex, "Error updating member role in organization {OrganizationId}", id);
                return StatusCode(500, new { message = "An error occurred during role update" });
            }
        }
    }
}
