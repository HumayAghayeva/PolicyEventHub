namespace PolicyEventHub.Applications.Diagnostics
{
    public interface ICorrelationIdAccessor
    {
        string? GetCorrelationId { get; }
    }
}
