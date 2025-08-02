using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Team
{
    public class CreateTeamDto
    {
        [Required]
        public int OrganizationId { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(7)]
        public string? Color { get; set; }
    }
}
