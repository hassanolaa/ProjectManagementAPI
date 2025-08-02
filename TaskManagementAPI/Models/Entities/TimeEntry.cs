using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class TimeEntry : BaseEntity
    {
        [Required]
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        public DateTime Date { get; set; }
        
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal Hours { get; set; }
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        public bool IsBillable { get; set; } = true;
        
        // Navigation Properties
        public virtual TaskItem Task { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
