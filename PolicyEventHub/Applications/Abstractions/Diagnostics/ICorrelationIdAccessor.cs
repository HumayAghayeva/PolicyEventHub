namespace PolicyEventHub.Applications.Abstractions.Diagnostics
{
    public interface ICorrelationIdAccessor
    {
        string? GetCorrelationId { get; }
    }
}
