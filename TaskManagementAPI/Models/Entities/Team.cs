using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TaskManagementAPI.Models.Entities
{
    public class Team : BaseEntity
    {
        [Required]
        [ForeignKey("Organization")]
        public int OrganizationId { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(7)]
        public string? Color { get; set; } // Hex color for UI
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public virtual Organization Organization { get; set; } = null!;
        public virtual ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
