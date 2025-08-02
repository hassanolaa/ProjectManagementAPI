

using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.Models.Entities
{
    public class Organization : BaseEntity
    {
       [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(1000)]
        public string? Description { get; set; }
        
        [StringLength(500)]
        public string? LogoUrl { get; set; }
        
        [StringLength(100)]
        public string? Website { get; set; }
        
        [Required]
        [StringLength(50)]
        public string SubscriptionPlan { get; set; } = "Free"; // Free, Basic, Premium, Enterprise
        
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public virtual ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}