using TaskManagementAPI.Models.DTOs.Organization;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Repository.Interfaces;
using TaskManagementAPI.Services.Interfaces;
using TaskManagementAPI.Helpers;
using TaskManagementAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementAPI.Services.Implementations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrganizationService> _logger;
        private readonly ApplicationDbContext _context; // Add this for direct context access

        public OrganizationService(
            IUnitOfWork unitOfWork, 
            ILogger<OrganizationService> logger,
            ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _context = context;
        }

        public async Task<OrganizationDto> CreateAsync(CreateOrganizationDto dto, string creatorUserId)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var organization = new Organization
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Website = dto.Website,
                    SubscriptionPlan = "Free",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Organizations.AddAsync(organization);
                await _unitOfWork.SaveChangesAsync();

                // Add creator as owner
                var organizationMember = new OrganizationMember
                {
                    OrganizationId = organization.Id,
                    UserId = creatorUserId,
                    Role = "Owner",
                    JoinedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                // Add member directly to context since we don't have OrganizationMember repository
                await _context.OrganizationMembers.AddAsync(organizationMember);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Organization {OrganizationId} created by user {UserId}", 
                    organization.Id, creatorUserId);

                // Get user role and member count for DTO
                var userRole = await _unitOfWork.Organizations.GetUserRoleAsync(organization.Id, creatorUserId);
                var memberCount = await _context.OrganizationMembers
                    .CountAsync(m => m.OrganizationId == organization.Id && m.IsActive);

                return OrganizationMappingHelper.MapToDto(organization, userRole ?? "", memberCount);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrganizationDto?> GetByIdAsync(int id, string userId)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            if (organization == null) return null;

            // Check if user is member
            var isMember = await _unitOfWork.Organizations.IsUserMemberAsync(id, userId);
            if (!isMember) return null;

            // Get additional data for DTO
            var userRole = await _unitOfWork.Organizations.GetUserRoleAsync(id, userId);
            var memberCount = await _context.OrganizationMembers
                .CountAsync(m => m.OrganizationId == id && m.IsActive);

            return OrganizationMappingHelper.MapToDto(organization, userRole ?? "", memberCount);
        }

        public async Task<IEnumerable<OrganizationDto>> GetUserOrganizationsAsync(string userId)
        {
            var organizations = await _unitOfWork.Organizations.GetUserOrganizationsAsync(userId);
            var result = new List<OrganizationDto>();

            foreach (var org in organizations)
            {
                var userRole = await _unitOfWork.Organizations.GetUserRoleAsync(org.Id, userId);
                var memberCount = await _context.OrganizationMembers
                    .CountAsync(m => m.OrganizationId == org.Id && m.IsActive);

                result.Add(OrganizationMappingHelper.MapToDto(org, userRole ?? "", memberCount));
            }

            return result;
        }

        public async Task<OrganizationDto> UpdateAsync(int id, UpdateOrganizationDto dto, string userId)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            if (organization == null)
                throw new KeyNotFoundException("Organization not found");

            // Check permissions
            var userRole = await _unitOfWork.Organizations.GetUserRoleAsync(id, userId);
            if (userRole != "Owner" && userRole != "Admin")
                throw new UnauthorizedAccessException("Insufficient permissions to update organization");

            // Update fields
            organization.Name = dto.Name ?? organization.Name;
            organization.Description = dto.Description ?? organization.Description;
            organization.Website = dto.Website ?? organization.Website;
            organization.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Organizations.UpdateAsync(organization);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Organization {OrganizationId} updated by user {UserId}", id, userId);

            // Get member count for DTO
            var memberCount = await _context.OrganizationMembers
                .CountAsync(m => m.OrganizationId == id && m.IsActive);

            return OrganizationMappingHelper.MapToDto(organization, userRole, memberCount);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            if (organization == null)
                throw new KeyNotFoundException("Organization not found");

            // Check permissions (only owner can delete)
            var userRole = await _unitOfWork.Organizations.GetUserRoleAsync(id, userId);
            if (userRole != "Owner")
                throw new UnauthorizedAccessException("Only organization owner can delete the organization");

            organization.IsActive = false;
            organization.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Organizations.UpdateAsync(organization);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Organization {OrganizationId} deleted by user {UserId}", id, userId);
        }

        public async Task<OrganizationMemberDto> AddMemberAsync(int organizationId, AddMemberDto dto, string adminUserId)
        {
            // Check permissions
            var adminRole = await _unitOfWork.Organizations.GetUserRoleAsync(organizationId, adminUserId);
            if (adminRole != "Owner" && adminRole != "Admin")
                throw new UnauthorizedAccessException("Insufficient permissions to add members");

            // Check if organization exists
            var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId);
            if (organization == null)
                throw new KeyNotFoundException("Organization not found");

            // Check if user is already a member
            var isAlreadyMember = await _unitOfWork.Organizations.IsUserMemberAsync(organizationId, dto.UserId);
            if (isAlreadyMember)
                throw new InvalidOperationException("User is already a member of this organization");

            var member = new OrganizationMember
            {
                OrganizationId = organizationId,
                UserId = dto.UserId,
                Role = dto.Role,
                JoinedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add member directly to context
            await _context.OrganizationMembers.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} added to organization {OrganizationId} by {AdminUserId}", 
                dto.UserId, organizationId, adminUserId);

            // Load user information for DTO
            var memberWithUser = await _context.OrganizationMembers
                .Include(m => m.User)
                .FirstAsync(m => m.Id == member.Id);

            return OrganizationMappingHelper.MapMemberToDto(memberWithUser);
        }

        public async Task RemoveMemberAsync(int organizationId, string memberUserId, string adminUserId)
        {
            // Check permissions
            var adminRole = await _unitOfWork.Organizations.GetUserRoleAsync(organizationId, adminUserId);
            if (adminRole != "Owner" && adminRole != "Admin")
                throw new UnauthorizedAccessException("Insufficient permissions to remove members");

            // Prevent owner from removing themselves
            if (memberUserId == adminUserId)
            {
                var memberRole = await _unitOfWork.Organizations.GetUserRoleAsync(organizationId, memberUserId);
                if (memberRole == "Owner")
                    throw new InvalidOperationException("Organization owner cannot remove themselves");
            }

            // Find the member
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && 
                                         m.UserId == memberUserId && 
                                         m.IsActive);

            if (member == null)
                throw new KeyNotFoundException("Member not found in this organization");

            // Soft delete - set IsActive to false
            member.IsActive = false;
            member.UpdatedAt = DateTime.UtcNow;

            _context.OrganizationMembers.Update(member);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} removed from organization {OrganizationId} by {AdminUserId}", 
                memberUserId, organizationId, adminUserId);
        }

        public async Task<IEnumerable<OrganizationMemberDto>> GetMembersAsync(int organizationId, string userId)
        {
            // Check if user has access to organization
            var isMember = await _unitOfWork.Organizations.IsUserMemberAsync(organizationId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this organization");

            var members = await _context.OrganizationMembers
                .AsNoTracking()
                .Include(m => m.User)
                .Where(m => m.OrganizationId == organizationId && m.IsActive)
                .OrderBy(m => m.JoinedAt)
                .ToListAsync();

            return members.Select(OrganizationMappingHelper.MapMemberToDto);
        }

        public async Task UpdateMemberRoleAsync(int organizationId, string memberUserId, string newRole, string adminUserId)
        {
            // Check permissions
            var adminRole = await _unitOfWork.Organizations.GetUserRoleAsync(organizationId, adminUserId);
            if (adminRole != "Owner" && adminRole != "Admin")
                throw new UnauthorizedAccessException("Insufficient permissions to update member roles");

            // Prevent changing owner role unless admin is also owner
            var currentMemberRole = await _unitOfWork.Organizations.GetUserRoleAsync(organizationId, memberUserId);
            if (currentMemberRole == "Owner" && adminRole != "Owner")
                throw new UnauthorizedAccessException("Only organization owner can change owner role");

            // Prevent owner from changing their own role
            if (memberUserId == adminUserId && currentMemberRole == "Owner")
                throw new InvalidOperationException("Organization owner cannot change their own role");

            // Find the member
            var member = await _context.OrganizationMembers
                .FirstOrDefaultAsync(m => m.OrganizationId == organizationId && 
                                         m.UserId == memberUserId && 
                                         m.IsActive);

            if (member == null)
                throw new KeyNotFoundException("Member not found in this organization");

            // Validate new role
            var validRoles = new[] { "Owner", "Admin", "Manager", "Member" };
            if (!validRoles.Contains(newRole))
                throw new ArgumentException($"Invalid role. Valid roles are: {string.Join(", ", validRoles)}");

            member.Role = newRole;
            member.UpdatedAt = DateTime.UtcNow;

            _context.OrganizationMembers.Update(member);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} role updated to {NewRole} in organization {OrganizationId} by {AdminUserId}", 
                memberUserId, newRole, organizationId, adminUserId);
        }

        public async Task<bool> HasPermissionAsync(int organizationId, string userId, string permission)
        {
            var userRole = await _unitOfWork.Organizations.GetUserRoleAsync(organizationId, userId);
            
            return permission switch
            {
                "read" => userRole != null,
                "write" => userRole is "Owner" or "Admin" or "Manager",
                "delete" => userRole is "Owner" or "Admin",
                "manage_members" => userRole is "Owner" or "Admin",
                "manage_roles" => userRole is "Owner" or "Admin",
                "delete_organization" => userRole is "Owner",
                _ => false
            };
        }
    }
}
