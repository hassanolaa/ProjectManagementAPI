using TaskManagementAPI.Models.DTOs.Team;
using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Helpers
{
    public static class TeamMappingHelper
    {
        public static TeamDto MapToDto(Team team, string userRole = "", int memberCount = 0)
        {
            return new TeamDto
            {
                Id = team.Id,
                OrganizationId = team.OrganizationId,
                OrganizationName = team.Organization?.Name ?? "",
                Name = team.Name,
                Description = team.Description,
                Color = team.Color,
                IsActive = team.IsActive,
                CreatedAt = team.CreatedAt,
                MemberCount = memberCount,
                UserRole = userRole
            };
        }

        public static TeamMemberDto MapMemberToDto(TeamMember member)
        {
            return new TeamMemberDto
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
