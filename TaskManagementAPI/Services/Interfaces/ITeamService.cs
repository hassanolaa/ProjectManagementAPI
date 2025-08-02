using TaskManagementAPI.Models.DTOs.Team;

namespace TaskManagementAPI.Services.Interfaces
{
    public interface ITeamService
    {
        Task<TeamDto> CreateAsync(CreateTeamDto dto, string userId);
        Task<TeamDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<TeamDto>> GetByOrganizationAsync(int organizationId, string userId);
        Task<IEnumerable<TeamDto>> GetUserTeamsAsync(string userId);
        Task<TeamDto> UpdateAsync(int id, UpdateTeamDto dto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<TeamMemberDto> AddMemberAsync(int teamId, AddTeamMemberDto dto, string userId);
        Task RemoveMemberAsync(int teamId, string memberUserId, string userId);
        Task<IEnumerable<TeamMemberDto>> GetMembersAsync(int teamId, string userId);
        Task UpdateMemberRoleAsync(int teamId, string memberUserId, string newRole, string userId);
    }
}
