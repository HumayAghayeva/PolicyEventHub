
namespace PolicyEventHub.Applications.Observability
{
    public interface IAppMetrics
    {
        void IncrementBusinessError(String reason);
        IDisposable MeasureUseCase(string useCaseName);
    }
}
