namespace PolicyEventHub.Applications.Observability
{
    public interface IAppTracer
    {
        IDisposable StartSpan(string name, IDictionary<string, object>? tags = null);
    }
}
