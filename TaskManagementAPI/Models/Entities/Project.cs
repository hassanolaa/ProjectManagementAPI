using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class Project : BaseEntity
    {
        [Required]
        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }
        
        [ForeignKey("Team")]
        public int? TeamId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(2000)]
        public string? Description { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Planning"; // Planning, Active, OnHold, Completed, Cancelled
        
        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
        
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DueDate { get; set; }
        
        [Column(TypeName = "decimal(5,2)")]
        public decimal? CompletionPercentage { get; set; } = 0;
        
        [StringLength(7)]
        public string? Color { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public virtual Organization Organization { get; set; } = null!;
        public virtual Team? Team { get; set; }
        public virtual ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public virtual ICollection<TaskStatus> TaskStatuses { get; set; } = new List<TaskStatus>();
    }
}
