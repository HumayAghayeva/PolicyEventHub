using System.Diagnostics.Metrics;

namespace PolicyEventHub.Infrastructure.Observability
{
    public static class AppMetrics
    {
        public static readonly Meter Meter = new("App.Metrics");

        public static readonly Counter<long> BusinessErrors =
            Meter.CreateCounter<long>("business_errors_total");

        public static readonly Histogram<double> UseCaseDuration =
            Meter.CreateHistogram<double>("usecase_duration_ms");
    }
}
