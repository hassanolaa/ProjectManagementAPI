using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Organization
{
    public class AddMemberDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Member"; // Owner, Admin, Member
    }
}
