using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Task
{
    public class UpdateTaskStatusDto
    {
        [Required]
        public int StatusId { get; set; }
    }
}
