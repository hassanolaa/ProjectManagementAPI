using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.TaskStatus
{
    public class CreateTaskStatusDto
    {
        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(7)]
        public string Color { get; set; } = "#6B7280";

        public int Order { get; set; }

        public bool IsDefault { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
    }
}
