using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.TaskStatus
{
    public class UpdateTaskStatusDto
    {
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(7)]
        public string? Color { get; set; }

        public int? Order { get; set; }

        public bool? IsDefault { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
