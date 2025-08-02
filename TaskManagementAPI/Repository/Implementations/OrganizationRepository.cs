using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Repository.Interfaces;

namespace TaskManagementAPI.Repository.Implementations
{
    public class OrganizationRepository : GenericRepository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Organization>> GetUserOrganizationsAsync(string userId)
        {
            return await _context.Organizations
                .AsNoTracking()
                .Where(o => o.Members.Any(m => m.UserId == userId && m.IsActive))
                .ToListAsync();
        }

        public async Task<Organization?> GetWithMembersAsync(int organizationId)
        {
            return await _context.Organizations
                .Include(o => o.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(o => o.Id == organizationId);
        }

        public async Task<bool> IsUserMemberAsync(int organizationId, string userId)
        {
            return await _context.OrganizationMembers
                .AnyAsync(m => m.OrganizationId == organizationId && m.UserId == userId && m.IsActive);
        }

        public async Task<string?> GetUserRoleAsync(int organizationId, string userId)
        {
            var member = await _context.OrganizationMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && m.UserId == userId && m.IsActive);
            
            return member?.Role;
        }

        public async Task<IEnumerable<Organization>> GetActiveOrganizationsAsync()
        {
            return await _context.Organizations
                .AsNoTracking()
                .Where(o => o.IsActive)
                .ToListAsync();
        }
    }
}
