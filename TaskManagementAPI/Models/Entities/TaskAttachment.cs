using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class TaskAttachment : BaseEntity
    {
        [Required]
        [ForeignKey("Task")]
        public int TaskId { get; set; }
        
        [Required]
        [ForeignKey("UploadedByUser")]
        public string UploadedByUserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;
        
        [Required]
        public long FileSize { get; set; }
        
        [StringLength(100)]
        public string? ContentType { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        // Navigation Properties
        public virtual TaskItem Task { get; set; } = null!;
        public virtual ApplicationUser UploadedByUser { get; set; } = null!;
    }
}
