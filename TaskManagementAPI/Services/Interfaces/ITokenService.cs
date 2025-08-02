using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Services.Interfaces
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync();
        Task<bool> ValidateRefreshTokenAsync(string token, string userId);
    }
}
