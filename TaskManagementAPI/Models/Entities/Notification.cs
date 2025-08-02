using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class Notification : BaseEntity
    {
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(1000)]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // TaskAssigned, TaskCompleted, ProjectUpdate, etc.
        
        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // Task, Project, Team, System
        
        public bool IsRead { get; set; } = false;
        
        public DateTime? ReadAt { get; set; }
        
        [StringLength(500)]
        public string? ActionUrl { get; set; }
        
        public int? RelatedEntityId { get; set; }
        
        [StringLength(50)]
        public string? RelatedEntityType { get; set; }
        
        [StringLength(1000)]
        public string? Metadata { get; set; } // JSON for additional data
        
        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
