using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class TeamMember : BaseEntity
    {
        [Required]
        [ForeignKey("Team")]
        public int TeamId { get; set; }
        
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Member"; // Lead, Member
        
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public virtual Team Team { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
