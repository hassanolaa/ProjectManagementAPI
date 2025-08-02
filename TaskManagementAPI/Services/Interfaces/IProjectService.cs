using TaskManagementAPI.Models.DTOs.Project;

namespace TaskManagementAPI.Services.Interfaces
{
    public interface IProjectService
    {
        Task<ProjectDto> CreateAsync(CreateProjectDto dto, string userId);
        Task<ProjectDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<ProjectDto>> GetByOrganizationAsync(int organizationId, string userId);
        Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId);
        Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<ProjectMemberDto> AddMemberAsync(int projectId, AddProjectMemberDto dto, string userId);
        Task RemoveMemberAsync(int projectId, string memberUserId, string userId);
        Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(int projectId, string userId);
        Task<ProjectStatsDto> GetProjectStatsAsync(int projectId, string userId);
        Task<ProjectDto> UpdateStatusAsync(int projectId, string status, string userId);
        Task<IEnumerable<ProjectDto>> GetActiveProjectsAsync(int organizationId, string userId);
    }
}
