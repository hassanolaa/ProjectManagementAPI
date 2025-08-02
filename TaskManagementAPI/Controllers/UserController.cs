using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TaskManagementAPI.Helpers;
using TaskManagementAPI.Models.DTOs.Auth;
using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        [HttpPut("profile")]
        [EnableRateLimiting("TaskCreationPolicy")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Update profile fields
                user.FirstName = dto.FirstName ?? user.FirstName;
                user.LastName = dto.LastName ?? user.LastName;
                user.PhoneNumber = dto.PhoneNumber;
                user.JobTitle = dto.JobTitle;
                user.Department = dto.Department;
                user.Bio = dto.Bio;
                user.UpdatedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return BadRequest(MappingHelper.CreateErrorResponse(
                        "Profile update failed.", result.Errors));
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userProfile = MappingHelper.MapToUserProfileDto(user, roles);

                _logger.LogInformation("Profile updated for user {UserId}", userId);

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, "An error occurred while updating your profile.");
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("change-password")]
        [EnableRateLimiting("AuthPolicy")]
        public async Task<ActionResult<AuthResponseDto>> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("id")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed for user {UserId}", userId);
                    
                    return Ok(MappingHelper.CreateSuccessResponse(
                        "Password changed successfully."));
                }

                return BadRequest(MappingHelper.CreateErrorResponse(
                    "Password change failed.", result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "An error occurred while changing your password.");
            }
        }
    }
}
