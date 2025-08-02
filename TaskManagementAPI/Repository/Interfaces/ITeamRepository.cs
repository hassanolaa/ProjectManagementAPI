using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Repository.Interfaces
{
    public interface ITeamRepository : IGenericRepository<Team>
    {
        Task<IEnumerable<Team>> GetByOrganizationAsync(int organizationId);
        Task<Team?> GetWithMembersAsync(int teamId);
        Task<bool> IsUserMemberAsync(int teamId, string userId);
        Task<IEnumerable<Team>> GetUserTeamsAsync(string userId);
        Task<IEnumerable<Team>> GetActiveTeamsByOrganizationAsync(int organizationId);
    }
}
