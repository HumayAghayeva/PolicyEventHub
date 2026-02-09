using PolicyEventHub.Applications.Observability;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using PolicyEventHub.Infrastructure.Utility;

namespace PolicyEventHub.Infrastructure.Observability
{
    public sealed class OpenTelemetryMetrics : IAppMetrics
    {
        private static readonly Histogram<double> _useCaseDuration =
            AppMetrics.Meter.CreateHistogram<double>(
                "usecase_duration_ms",
                unit: "ms",
                description: "Use case execution duration");

        private static readonly Counter<long> _businessErrors =
            AppMetrics.Meter.CreateCounter<long>(
                "business_errors_total",
                description: "Business-level error counter");

        public IDisposable MeasureUseCase(string useCaseName)
        {
            var start = Stopwatch.GetTimestamp();

            return new DisposableAction(() =>
            {
                var elapsedMs =
                    (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;

                _useCaseDuration.Record(
                    elapsedMs,
                    new KeyValuePair<string, object?>("usecase", useCaseName));
            });
        }


        public void IncrementBusinessError(string reason)
        {
            _businessErrors.Add(
                1,
                new KeyValuePair<string, object?>("reason", reason));
        }
    }
}
