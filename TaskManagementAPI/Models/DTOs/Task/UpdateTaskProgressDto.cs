using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Task
{
    public class UpdateTaskProgressDto
    {
        [Required]
        [Range(0, 100)]
        public decimal CompletionPercentage { get; set; }
    }
}
