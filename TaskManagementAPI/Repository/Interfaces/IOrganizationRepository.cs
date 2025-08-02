using TaskManagementAPI.Models.Entities;

namespace TaskManagementAPI.Repository.Interfaces
{
    public interface IOrganizationRepository : IGenericRepository<Organization>
    {
        Task<IEnumerable<Organization>> GetUserOrganizationsAsync(string userId);
        Task<Organization?> GetWithMembersAsync(int organizationId);
        Task<bool> IsUserMemberAsync(int organizationId, string userId);
        Task<string?> GetUserRoleAsync(int organizationId, string userId);
        Task<IEnumerable<Organization>> GetActiveOrganizationsAsync();
    }
}
