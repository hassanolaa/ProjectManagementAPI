using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Organization
{
    public class UpdateOrganizationDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Website { get; set; }
    }
}
