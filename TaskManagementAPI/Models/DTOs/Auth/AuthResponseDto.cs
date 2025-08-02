namespace TaskManagementAPI.Models.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? TokenExpiry { get; set; }
        public UserProfileDto? User { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
