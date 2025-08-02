using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class TaskComment : BaseEntity
    {
        [Required]
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        
        [ForeignKey("ParentComment")]
        public int? ParentCommentId { get; set; }
        
        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = string.Empty;
        
        public bool IsEdited { get; set; } = false;
        public DateTime? EditedAt { get; set; }
        
        // Navigation Properties
        public virtual TaskItem Task { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual TaskComment? ParentComment { get; set; }
        public virtual ICollection<TaskComment> Replies { get; set; } = new List<TaskComment>();
    }
}
