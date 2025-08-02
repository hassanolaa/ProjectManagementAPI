using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Project
{
    public class UpdateProjectDto
    {
        [StringLength(200)]
        public string? Name { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DueDate { get; set; }

        [StringLength(7)]
        public string? Color { get; set; }
    }
}
