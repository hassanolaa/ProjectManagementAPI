using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Repository.Interfaces
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<IEnumerable<Project>> GetByOrganizationAsync(int organizationId);
        Task<IEnumerable<Project>> GetByTeamAsync(int teamId);
        Task<IEnumerable<Project>> GetUserProjectsAsync(string userId);
        Task<Project?> GetWithTasksAsync(int projectId);
        Task<Project?> GetWithMembersAsync(int projectId);
        Task<bool> IsUserMemberAsync(int projectId, string userId);
        Task<string?> GetUserRoleAsync(int projectId, string userId);
        Task<IEnumerable<Project>> GetActiveProjectsAsync(int organizationId);
        Task UpdateCompletionPercentageAsync(int projectId);
    }
}
