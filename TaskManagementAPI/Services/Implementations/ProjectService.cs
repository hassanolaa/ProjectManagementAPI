using TaskManagementAPI.Models.DTOs.Project;
using TaskManagementAPI.Models.Entities;
using TaskManagementAPI.Repository.Interfaces;
using TaskManagementAPI.Services.Interfaces;
using TaskManagementAPI.Helpers;
using TaskManagementAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManagementAPI.Services.Implementations
{
    public class ProjectService : IProjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskStatusService _taskStatusService;
        private readonly ILogger<ProjectService> _logger;
        private readonly ApplicationDbContext _context; // Add for direct context access
        private readonly ICacheService _cacheService;

        private const string PROJECT_CACHE_KEY = "project:{0}";
        private const string USER_PROJECTS_CACHE_KEY = "user_projects:{0}";
        private const string ORG_PROJECTS_CACHE_KEY = "org_projects:{0}";


        public ProjectService(
            IUnitOfWork unitOfWork, 
            ITaskStatusService taskStatusService,
            ILogger<ProjectService> logger,
            ApplicationDbContext context,
            ICacheService cacheService
            )
        {
            _unitOfWork = unitOfWork;
            _taskStatusService = taskStatusService;
            _logger = logger;
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, string userId)
        {
            // Verify user has access to organization
            var isMember = await _unitOfWork.Organizations.IsUserMemberAsync(dto.OrganizationId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this organization");

            // Verify team belongs to organization if teamId is provided
            if (dto.TeamId.HasValue)
            {
                var team = await _unitOfWork.Teams.GetByIdAsync(dto.TeamId.Value);
                if (team == null || team.OrganizationId != dto.OrganizationId)
                    throw new InvalidOperationException("Team does not belong to the specified organization");
            }

            var project = new Project
            {
                Name = dto.Name,
                Description = dto.Description,
                OrganizationId = dto.OrganizationId,
                TeamId = dto.TeamId,
                Status = "Planning",
                Priority = dto.Priority ?? "Medium",
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                Color = dto.Color,
                CompletionPercentage = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Projects.AddAsync(project);
                await _unitOfWork.SaveChangesAsync();

                // Add creator as project manager
                var projectMember = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = userId,
                    Role = "Manager",
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.ProjectMembers.AddAsync(projectMember);

                // Create default task statuses for the project
                await _taskStatusService.CreateDefaultStatusesAsync(project.Id, userId);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Project {ProjectId} created by user {UserId}", project.Id, userId);

                // Get additional data for DTO
                var userRole = await _unitOfWork.Projects.GetUserRoleAsync(project.Id, userId);
                var taskCount = 0; // New project has no tasks
                var memberCount = 1; // Creator is the first member

                return ProjectMappingHelper.MapToDto(project, userRole ?? "", taskCount, memberCount);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ProjectDto?> GetByIdAsync(int id, string userId)
        {
            var cacheKey = string.Format(PROJECT_CACHE_KEY, id);
            var cachedProject = await _cacheService.GetAsync<ProjectDto>(cacheKey);

            if (cachedProject != null)
            {
                return cachedProject;
            }

            // Get from database and cache
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null) return null;

            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(id, userId);
            if (!isMember) return null;

            var projectDto = ProjectMappingHelper.MapToDto(project);
            await _cacheService.SetAsync(cacheKey, projectDto, TimeSpan.FromHours(1));

            return projectDto;
        }

        public async Task<IEnumerable<ProjectDto>> GetUserProjectsAsync(string userId)
        {
            var projects = await _unitOfWork.Projects.GetUserProjectsAsync(userId);
            var result = new List<ProjectDto>();

            foreach (var project in projects)
            {
                var userRole = await _unitOfWork.Projects.GetUserRoleAsync(project.Id, userId);
                var taskCount = await _context.Tasks.CountAsync(t => t.ProjectId == project.Id && t.IsActive);
                var memberCount = await _context.ProjectMembers.CountAsync(m => m.ProjectId == project.Id && m.IsActive);

                result.Add(ProjectMappingHelper.MapToDto(project, userRole ?? "", taskCount, memberCount));
            }

            return result;
        }

        public async Task<IEnumerable<ProjectDto>> GetByOrganizationAsync(int organizationId, string userId)
        {
            // Check if user has access to organization
            var isMember = await _unitOfWork.Organizations.IsUserMemberAsync(organizationId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this organization");

            var projects = await _unitOfWork.Projects.GetByOrganizationAsync(organizationId);
            var result = new List<ProjectDto>();

            foreach (var project in projects)
            {
                // Only include projects where user is a member
                var isProjectMember = await _unitOfWork.Projects.IsUserMemberAsync(project.Id, userId);
                if (isProjectMember)
                {
                    var userRole = await _unitOfWork.Projects.GetUserRoleAsync(project.Id, userId);
                    var taskCount = await _context.Tasks.CountAsync(t => t.ProjectId == project.Id && t.IsActive);
                    var memberCount = await _context.ProjectMembers.CountAsync(m => m.ProjectId == project.Id && m.IsActive);

                    result.Add(ProjectMappingHelper.MapToDto(project, userRole ?? "", taskCount, memberCount));
                }
            }

            return result;
        }

        public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto dto, string userId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
                throw new KeyNotFoundException("Project not found");

            // Check permissions - user must be project manager or organization admin
            var userRole = await _unitOfWork.Projects.GetUserRoleAsync(id, userId);
            var orgRole = await _unitOfWork.Organizations.GetUserRoleAsync(project.OrganizationId, userId);
            
            if (userRole != "Manager" && orgRole != "Owner" && orgRole != "Admin")
                throw new UnauthorizedAccessException("Insufficient permissions to update project");

            // Update fields
            if (!string.IsNullOrEmpty(dto.Name))
                project.Name = dto.Name;

            if (dto.Description != null)
                project.Description = dto.Description;

            if (!string.IsNullOrEmpty(dto.Priority))
                project.Priority = dto.Priority;

            if (dto.StartDate != null)
                project.StartDate = dto.StartDate;

            if (dto.EndDate != null)
                project.EndDate = dto.EndDate;

            if (dto.DueDate != null)
                project.DueDate = dto.DueDate;

            if (!string.IsNullOrEmpty(dto.Color))
                project.Color = dto.Color;

            project.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Project {ProjectId} updated by user {UserId}", id, userId);

            // Get additional data for DTO
            var taskCount = await _context.Tasks.CountAsync(t => t.ProjectId == id && t.IsActive);
            var memberCount = await _context.ProjectMembers.CountAsync(m => m.ProjectId == id && m.IsActive);

            return ProjectMappingHelper.MapToDto(project, userRole ?? "", taskCount, memberCount);
        }

        public async Task DeleteAsync(int id, string userId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
                throw new KeyNotFoundException("Project not found");

            // Check permissions - only project manager or organization owner can delete
            var userRole = await _unitOfWork.Projects.GetUserRoleAsync(id, userId);
            var orgRole = await _unitOfWork.Organizations.GetUserRoleAsync(project.OrganizationId, userId);
            
            if (userRole != "Manager" && orgRole != "Owner")
                throw new UnauthorizedAccessException("Only project manager or organization owner can delete project");

            // Soft delete - set IsActive to false
            project.IsActive = false;
            project.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Project {ProjectId} deleted by user {UserId}", id, userId);
        }

        public async Task<ProjectMemberDto> AddMemberAsync(int projectId, AddProjectMemberDto dto, string userId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException("Project not found");

            // Check permissions - user must be project manager or organization admin
            var userRole = await _unitOfWork.Projects.GetUserRoleAsync(projectId, userId);
            var orgRole = await _unitOfWork.Organizations.GetUserRoleAsync(project.OrganizationId, userId);
            
            if (userRole != "Manager" && orgRole != "Owner" && orgRole != "Admin")
                throw new UnauthorizedAccessException("Insufficient permissions to add members");

            // Check if user to be added is organization member
            var isOrgMember = await _unitOfWork.Organizations.IsUserMemberAsync(project.OrganizationId, dto.UserId);
            if (!isOrgMember)
                throw new InvalidOperationException("User must be an organization member to be added to project");

            // Check if user is already a project member
            var isAlreadyMember = await _unitOfWork.Projects.IsUserMemberAsync(projectId, dto.UserId);
            if (isAlreadyMember)
                throw new InvalidOperationException("User is already a member of this project");

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = dto.UserId,
                Role = dto.Role,
                AssignedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.ProjectMembers.AddAsync(member);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} added to project {ProjectId} by {AdminUserId}", 
                dto.UserId, projectId, userId);

            // Load user information for DTO
            var memberWithUser = await _context.ProjectMembers
                .Include(m => m.User)
                .FirstAsync(m => m.Id == member.Id);

            return ProjectMappingHelper.MapMemberToDto(memberWithUser);
        }

        public async Task RemoveMemberAsync(int projectId, string memberUserId, string userId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException("Project not found");

            // Check permissions
            var userRole = await _unitOfWork.Projects.GetUserRoleAsync(projectId, userId);
            var orgRole = await _unitOfWork.Organizations.GetUserRoleAsync(project.OrganizationId, userId);
            
            if (userRole != "Manager" && orgRole != "Owner" && orgRole != "Admin")
                throw new UnauthorizedAccessException("Insufficient permissions to remove members");

            // Prevent removing the last manager
            if (await IsLastManagerAsync(projectId, memberUserId))
                throw new InvalidOperationException("Cannot remove the last project manager");

            // Find the member
            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && 
                                         m.UserId == memberUserId && 
                                         m.IsActive);

            if (member == null)
                throw new KeyNotFoundException("Member not found in this project");

            // Soft delete - set IsActive to false
            member.IsActive = false;
            member.UpdatedAt = DateTime.UtcNow;

            _context.ProjectMembers.Update(member);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User {UserId} removed from project {ProjectId} by {AdminUserId}", 
                memberUserId, projectId, userId);
        }

        public async Task<IEnumerable<ProjectMemberDto>> GetMembersAsync(int projectId, string userId)
        {
            // Check if user has access to project
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(projectId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this project");

            var members = await _context.ProjectMembers
                .AsNoTracking()
                .Include(m => m.User)
                .Where(m => m.ProjectId == projectId && m.IsActive)
                .OrderBy(m => m.AssignedAt)
                .ToListAsync();

            return members.Select(ProjectMappingHelper.MapMemberToDto);
        }

        public async Task<ProjectDto> UpdateStatusAsync(int projectId, string status, string userId)
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException("Project not found");

            // Check permissions
            var userRole = await _unitOfWork.Projects.GetUserRoleAsync(projectId, userId);
            var orgRole = await _unitOfWork.Organizations.GetUserRoleAsync(project.OrganizationId, userId);
            
            if (userRole != "Manager" && orgRole != "Owner" && orgRole != "Admin")
                throw new UnauthorizedAccessException("Insufficient permissions to update project status");

            // Validate status
            var validStatuses = new[] { "Planning", "Active", "OnHold", "Completed", "Cancelled" };
            if (!validStatuses.Contains(status))
                throw new ArgumentException($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}");

            project.Status = status;
            project.UpdatedAt = DateTime.UtcNow;

            // Set end date if completed
            if (status == "Completed" && project.EndDate == null)
            {
                project.EndDate = DateTime.UtcNow;
            }

            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Project {ProjectId} status updated to {Status} by user {UserId}", 
                projectId, status, userId);

            // Get additional data for DTO
            var taskCount = await _context.Tasks.CountAsync(t => t.ProjectId == projectId && t.IsActive);
            var memberCount = await _context.ProjectMembers.CountAsync(m => m.ProjectId == projectId && m.IsActive);

            return ProjectMappingHelper.MapToDto(project, userRole ?? "", taskCount, memberCount);
        }

        public async Task<IEnumerable<ProjectDto>> GetActiveProjectsAsync(int organizationId, string userId)
        {
            // Check if user has access to organization
            var isMember = await _unitOfWork.Organizations.IsUserMemberAsync(organizationId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this organization");

            var projects = await _unitOfWork.Projects.GetActiveProjectsAsync(organizationId);
            var result = new List<ProjectDto>();

            foreach (var project in projects)
            {
                // Only include projects where user is a member
                var isProjectMember = await _unitOfWork.Projects.IsUserMemberAsync(project.Id, userId);
                if (isProjectMember)
                {
                    var userRole = await _unitOfWork.Projects.GetUserRoleAsync(project.Id, userId);
                    var taskCount = await _context.Tasks.CountAsync(t => t.ProjectId == project.Id && t.IsActive);
                    var memberCount = await _context.ProjectMembers.CountAsync(m => m.ProjectId == project.Id && m.IsActive);

                    result.Add(ProjectMappingHelper.MapToDto(project, userRole ?? "", taskCount, memberCount));
                }
            }

            return result;
        }

        public async Task<ProjectStatsDto> GetProjectStatsAsync(int projectId, string userId)
        {
            // Check access
            var isMember = await _unitOfWork.Projects.IsUserMemberAsync(projectId, userId);
            if (!isMember)
                throw new UnauthorizedAccessException("You don't have access to this project");

            var project = await _unitOfWork.Projects.GetWithTasksAsync(projectId);
            if (project == null)
                throw new KeyNotFoundException("Project not found");

            var totalTasks = project.Tasks.Count(t => t.IsActive);
            var completedTasks = project.Tasks.Count(t => t.IsActive && t.CompletionPercentage >= 100);
            var overdueTasks = project.Tasks.Count(t => t.IsActive && 
                t.DueDate.HasValue && t.DueDate.Value < DateTime.UtcNow && t.CompletionPercentage < 100);

            return new ProjectStatsDto
            {
                ProjectId = projectId,
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                InProgressTasks = totalTasks - completedTasks,
                OverdueTasks = overdueTasks,
                CompletionPercentage = project.CompletionPercentage ?? 0
            };
        }

        // Helper method to check if user is the last manager
        private async Task<bool> IsLastManagerAsync(int projectId, string userId)
        {
            var managerCount = await _context.ProjectMembers
                .CountAsync(m => m.ProjectId == projectId && 
                                m.Role == "Manager" && 
                                m.IsActive);

            if (managerCount > 1)
                return false;

            var member = await _context.ProjectMembers
                .FirstOrDefaultAsync(m => m.ProjectId == projectId && 
                                         m.UserId == userId && 
                                         m.Role == "Manager" && 
                                         m.IsActive);

            return member != null;
        }
    }
}
