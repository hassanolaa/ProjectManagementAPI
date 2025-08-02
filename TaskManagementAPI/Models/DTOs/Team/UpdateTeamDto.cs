using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Team
{
    public class UpdateTeamDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(7)]
        public string? Color { get; set; }
    }
}
