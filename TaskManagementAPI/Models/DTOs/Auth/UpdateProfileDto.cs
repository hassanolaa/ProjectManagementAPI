using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Auth
{
    public class UpdateProfileDto
    {
        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        [Phone]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? JobTitle { get; set; }

        [StringLength(50)]
        public string? Department { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }
    }
}
