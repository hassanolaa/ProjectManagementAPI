using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class TaskItem : BaseEntity
    {
        [Required]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        
        [Required]
        [ForeignKey("TaskStatus")]
        public int TaskStatusId { get; set; }
        
        [ForeignKey("AssignedToUser")]
        public string? AssignedToUserId { get; set; }
        
        [Required]
        [ForeignKey("CreatedByUser")]
        public string CreatedByUserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [StringLength(5000)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        public DateTime? DueDate { get; set; }
        
        [Column(TypeName = "decimal(8,2)")]
        public decimal? EstimatedHours { get; set; }
        
        [Column(TypeName = "decimal(8,2)")]
        public decimal? ActualHours { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal CompletionPercentage { get; set; } = 0;
        
        public DateTime? StartDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        
        [StringLength(1000)]
        public string? Tags { get; set; } // JSON array of tags
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public virtual Project Project { get; set; } = null!;
        public virtual TaskStatus TaskStatus { get; set; } = null!;
        public virtual ApplicationUser? AssignedToUser { get; set; }
        public virtual ApplicationUser CreatedByUser { get; set; } = null!;
        public virtual ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
        public virtual ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
        public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
    }
}
