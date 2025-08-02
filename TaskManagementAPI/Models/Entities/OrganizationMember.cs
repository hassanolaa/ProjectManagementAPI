using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class OrganizationMember : BaseEntity
    {
        [Required]
        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }
        
        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "Member"; // Owner, Admin, Member
        
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public virtual Organization Organization { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
