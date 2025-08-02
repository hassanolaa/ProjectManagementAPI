using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Task
{
    public class CreateTaskDto
    {
        [Required]
        public int ProjectId { get; set; }

        public int TaskStatusId { get; set; } // 0 = use default status

        public string? AssignedToUserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(5000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; } = "Medium";

        public DateTime? DueDate { get; set; }

        public decimal? EstimatedHours { get; set; }

        [StringLength(1000)]
        public string? Tags { get; set; }
    }
}
