using Microsoft.Extensions.Diagnostics.Metrics;

namespace PolicyEventHub.Infrastructure.Configurations
{
    public sealed class ObservabilityOptions
    {
        public string ServiceName { get; set; } = "PolicyEventHub";
        public string? ServiceVersion { get; set; }

        public MetricsOptions Metrics { get; set; } = new();
        public TracingOptions Tracing { get; set; } = new();

        public List<string> IgnorePaths { get; set; }
    }
    public sealed class MetricsOptions
    {
        public bool Enabled { get; set; } = true;
        public string MeterName { get; set; } = "App.Metrics";
    }

    public sealed class TracingOptions
    {
        public bool Enabled { get; set; } = true;
        public string ActivitySourceName { get; set; } = "App.Tracing";
        public string? OtlpEndpoint { get; set; }
    }
}
