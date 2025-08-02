using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Task
{
    public class AssignTaskDto
    {
        public string? AssigneeId { get; set; } // null to unassign
    }
}
