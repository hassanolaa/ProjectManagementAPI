using TaskManagementAPI.Models.DTOs.Organization;
using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Helpers
{
    public static class OrganizationMappingHelper
    {
        public static OrganizationDto MapToDto(Organization organization, string userRole = "", int memberCount = 0)
        {
            return new OrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Description = organization.Description,
                Website = organization.Website,
                SubscriptionPlan = organization.SubscriptionPlan,
                IsActive = organization.IsActive,
                CreatedAt = organization.CreatedAt,
                MemberCount = memberCount,
                UserRole = userRole
            };
        }

        public static OrganizationMemberDto MapMemberToDto(OrganizationMember member)
        {
            return new OrganizationMemberDto
            {
                Id = member.Id,
                UserId = member.UserId,
                UserName = $"{member.User?.FirstName} {member.User?.LastName}",
                UserEmail = member.User?.Email ?? "",
                Role = member.Role,
                JoinedAt = member.JoinedAt,
                IsActive = member.IsActive
            };
        }
    }
}
