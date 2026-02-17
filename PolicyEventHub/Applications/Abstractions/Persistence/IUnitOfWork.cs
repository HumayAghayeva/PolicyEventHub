namespace PolicyEventHub.Applications.Abstractions.Persistence
{
    public interface IUnitOfWork
    {
        Task ExecuteAsync(Func<CancellationToken ,Task> action, CancellationToken cancellationToken);
    }
}
