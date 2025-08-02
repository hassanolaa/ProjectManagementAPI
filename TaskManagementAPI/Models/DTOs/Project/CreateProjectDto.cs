using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Project
{
    public class CreateProjectDto
    {
        [Required]
        public int OrganizationId { get; set; }

        public int? TeamId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; } = "Medium";

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }

        [StringLength(7)]
        public string? Color { get; set; }
    }
}
