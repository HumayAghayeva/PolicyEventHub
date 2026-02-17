namespace PolicyEventHub.Extensions
{
    public static class ObservabilityAppExtensions
    {
        public static WebApplication UseObservability(
            this WebApplication app)
        {
            app.MapPrometheusScrapingEndpoint("/metrics");

            return app;
        }
    }
}
