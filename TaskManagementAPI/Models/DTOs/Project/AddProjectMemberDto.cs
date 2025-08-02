using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Project
{
    public class AddProjectMemberDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Contributor"; // Manager, Contributor, Viewer
    }
}
