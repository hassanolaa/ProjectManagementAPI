// Models/DTOs/Organization/UpdateMemberRoleDto.cs
using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.DTOs.Organization
{
    public class UpdateMemberRoleDto
    {
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = string.Empty;
    }
}
