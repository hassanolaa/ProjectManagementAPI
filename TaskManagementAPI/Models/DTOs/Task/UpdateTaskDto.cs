using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Task
{
    public class UpdateTaskDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(5000)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public decimal? EstimatedHours { get; set; }

        [Range(0, 100)]
        public decimal? CompletionPercentage { get; set; }

        [StringLength(1000)]
        public string? Tags { get; set; }
    }
}
