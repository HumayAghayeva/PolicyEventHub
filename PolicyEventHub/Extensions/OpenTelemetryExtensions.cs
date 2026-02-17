using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PolicyEventHub.Infrastructure.Configurations;

namespace PolicyEventHub.Extensions
{
    public static class OpenTelemetryExtensions
    {
        public static IServiceCollection AddObservability(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var options = configuration
                .GetSection("Observability")
                .Get<ObservabilityOptions>()
                ?? new ObservabilityOptions();

            services.AddOpenTelemetry()
                .ConfigureResource(resource =>
                {
                    resource.AddService(
                        serviceName: options.ServiceName,
                        serviceVersion: options.ServiceVersion);
                })
                .WithMetrics(metrics =>
                {
                    if (!options.Metrics.Enabled)
                        return;

                    metrics
                      .AddAspNetCoreInstrumentation()
                      .AddHttpClientInstrumentation()
                      .AddRuntimeInstrumentation()
                      .AddProcessInstrumentation()
                      .AddMeter(options.Metrics.MeterName)
                      .AddPrometheusExporter();
                })
                .WithTracing(tracing =>
                {
                    if (!options.Tracing.Enabled)
                        return;

                    tracing
                        .AddAspNetCoreInstrumentation(o =>
                        {
                            o.Filter = ctx =>
                                !options.IgnorePaths
                                    .Any(p => ctx.Request.Path.StartsWithSegments(p, StringComparison.OrdinalIgnoreCase));
                        })
                        .AddHttpClientInstrumentation()
                        .AddSource(options.Tracing.ActivitySourceName);

                    if (!string.IsNullOrWhiteSpace(options.Tracing.OtlpEndpoint))
                    {
                        tracing.AddOtlpExporter(o =>
                        {
                            o.Endpoint = new Uri(options.Tracing.OtlpEndpoint);
                        });
                    }
                });

            return services;
        }
    }
}
