using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.Entities
{
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}




