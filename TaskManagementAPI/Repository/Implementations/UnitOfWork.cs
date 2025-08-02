using Microsoft.EntityFrameworkCore.Storage;
using TaskManagementAPI.Data;
using TaskManagementAPI.Repository.Interfaces;

namespace TaskManagementAPI.Repository.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Organizations = new OrganizationRepository(_context);
            Teams = new TeamRepository(_context);
            Projects = new ProjectRepository(_context);
            Tasks = new TaskRepository(_context);
            TaskStatuses = new TaskStatusRepository(_context);
        }

        public IOrganizationRepository Organizations { get; }
        public ITeamRepository Teams { get; }
        public IProjectRepository Projects { get; }
        public ITaskRepository Tasks { get; }
        public ITaskStatusRepository TaskStatuses { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
