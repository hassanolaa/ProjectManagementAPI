

using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.Data;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Repository.Interfaces;

namespace TaskManagementAPI.Repository.Implementations
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Team>> GetByOrganizationAsync(int organizationId)
        {
            return await _context.Teams
                .AsNoTracking()
                .Where(t => t.OrganizationId == organizationId && t.IsActive)
                .Include(t => t.Members)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<Team?> GetWithMembersAsync(int teamId)
        {
            return await _context.Teams
                .AsNoTracking()
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.Id == teamId && t.IsActive);
        }

        public async Task<bool> IsUserMemberAsync(int teamId, string userId)
        {
            return await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId && tm.IsActive);
        }

        public async Task<IEnumerable<Team>> GetUserTeamsAsync(string userId)
        {
            return await _context.TeamMembers
                .AsNoTracking()
                .Where(tm => tm.UserId == userId && tm.IsActive)
                .Select(tm => tm.Team)
                .Include(t => t.Organization)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetActiveTeamsByOrganizationAsync(int organizationId)
        {
            return await _context.Teams
                .AsNoTracking()
                .Where(t => t.OrganizationId == organizationId && t.IsActive)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }
    }
}