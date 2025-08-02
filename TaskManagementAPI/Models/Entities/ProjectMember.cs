using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class ProjectMember : BaseEntity
    {
        [Required]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Contributor"; // Manager, Contributor, Viewer
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public virtual Project Project { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
