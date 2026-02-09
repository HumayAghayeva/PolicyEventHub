using System.Diagnostics;

namespace PolicyEventHub.Infrastructure.Observability
{
    public static class AppActivitySource
    {
        public static readonly ActivitySource Instance =
            new("App.Tracing");
    }
}
