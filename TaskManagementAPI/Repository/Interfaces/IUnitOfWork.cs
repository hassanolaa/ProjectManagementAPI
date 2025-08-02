namespace TaskManagementAPI.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IOrganizationRepository Organizations { get; }
        ITeamRepository Teams { get; }
        IProjectRepository Projects { get; }
        ITaskRepository Tasks { get; }
        ITaskStatusRepository TaskStatuses { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
