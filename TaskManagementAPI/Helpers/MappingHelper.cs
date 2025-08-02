using Microsoft.AspNetCore.Identity;
using TaskManagementAPI.Models.DTOs.Auth;
using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Helpers
{
    public static class MappingHelper
    {
        public static ApplicationUser MapToApplicationUser(RegisterDto dto)
        {
            return new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                JobTitle = dto.JobTitle,
                Department = dto.Department,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailConfirmed = false // Will be confirmed via email
            };
        }

        public static UserProfileDto MapToUserProfileDto(ApplicationUser user, IList<string> roles)
        {
            return new UserProfileDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                JobTitle = user.JobTitle,
                Department = user.Department,
                ProfilePictureUrl = user.ProfilePictureUrl,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                LastActiveAt = user.LastActiveAt,
                IsActive = user.IsActive,
                Roles = roles.ToList()
            };
        }

        public static AuthResponseDto CreateSuccessResponse(string message, string? token = null, UserProfileDto? user = null)
        {
            return new AuthResponseDto
            {
                Success = true,
                Message = message,
                Token = token,
                TokenExpiry = token != null ? DateTime.UtcNow.AddHours(24) : null,
                User = user
            };
        }

        public static AuthResponseDto CreateErrorResponse(string message, IEnumerable<IdentityError>? errors = null)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = message,
                Errors = errors?.Select(e => e.Description).ToList() ?? new List<string>()
            };
        }

        public static AuthResponseDto CreateErrorResponse(string message, List<string> errors)
        {
            return new AuthResponseDto
            {
                Success = false,
                Message = message,
                Errors = errors
            };
        }
    }
}
