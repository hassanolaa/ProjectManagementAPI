using TaskManagementAPI.Models.DTOs.Organization;

namespace TaskManagementAPI.Services.Interfaces
{
    public interface IOrganizationService
    {
        Task<OrganizationDto> CreateAsync(CreateOrganizationDto dto, string creatorUserId);
        Task<OrganizationDto?> GetByIdAsync(int id, string userId);
        Task<IEnumerable<OrganizationDto>> GetUserOrganizationsAsync(string userId);
        Task<OrganizationDto> UpdateAsync(int id, UpdateOrganizationDto dto, string userId);
        Task DeleteAsync(int id, string userId);
        Task<OrganizationMemberDto> AddMemberAsync(int organizationId, AddMemberDto dto, string adminUserId);
        Task RemoveMemberAsync(int organizationId, string memberUserId, string adminUserId);
        Task<IEnumerable<OrganizationMemberDto>> GetMembersAsync(int organizationId, string userId);
        Task UpdateMemberRoleAsync(int organizationId, string memberUserId, string newRole, string adminUserId);
        Task<bool> HasPermissionAsync(int organizationId, string userId, string permission);
    }
}
