using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Team
{
    public class AddTeamMemberDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Member"; // Lead, Member
    }
}
