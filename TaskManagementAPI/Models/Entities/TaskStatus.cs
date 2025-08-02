using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class TaskStatus : BaseEntity
    {
        [Required]
        [ForeignKey("Project")]
        public int ProjectId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(7)]
        public string Color { get; set; } = "#6B7280";
        
        [Required]
        public int Order { get; set; }
        
        public bool IsDefault { get; set; } = false;
        public bool IsCompleted { get; set; } = false;
        
        // Navigation Properties
        public virtual Project Project { get; set; } = null!;
        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
