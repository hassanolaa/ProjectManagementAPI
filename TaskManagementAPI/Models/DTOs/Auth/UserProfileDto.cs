namespace TaskManagementAPI.Models.DTOs.Auth
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? JobTitle { get; set; }
        public string? Department { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActiveAt { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
