using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Project
{
    public class UpdateProjectStatusDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty;
    }
}
