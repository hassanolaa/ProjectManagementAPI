using TaskManagementAPI.Models.DTOs.Project;
using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Helpers
{
    public static class ProjectMappingHelper
    {
        public static ProjectDto MapToDto(Project project, string userRole = "", int taskCount = 0, int memberCount = 0)
        {
            return new ProjectDto
            {
                Id = project.Id,
                OrganizationId = project.OrganizationId,
                OrganizationName = project.Organization?.Name ?? "",
                TeamId = project.TeamId,
                TeamName = project.Team?.Name,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                Priority = project.Priority,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                DueDate = project.DueDate,
                CompletionPercentage = project.CompletionPercentage,
                Color = project.Color,
                IsActive = project.IsActive,
                CreatedAt = project.CreatedAt,
                TaskCount = taskCount,
                MemberCount = memberCount,
                UserRole = userRole
            };
        }

        public static ProjectMemberDto MapMemberToDto(ProjectMember member)
        {
            return new ProjectMemberDto
            {
                Id = member.Id,
                UserId = member.UserId,
                UserName = $"{member.User?.FirstName} {member.User?.LastName}",
                UserEmail = member.User?.Email ?? "",
                Role = member.Role,
                AssignedAt = member.AssignedAt,
                IsActive = member.IsActive
            };
        }
    }
}
