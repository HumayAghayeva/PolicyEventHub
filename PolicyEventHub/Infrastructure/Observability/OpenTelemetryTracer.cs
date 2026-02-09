using PolicyEventHub.Applications.Observability;
using PolicyEventHub.Infrastructure.Utility;
using System.Diagnostics;

namespace PolicyEventHub.Infrastructure.Observability
{
    public sealed class OpenTelemetryTracer : IAppTracer
    {
        public IDisposable StartSpan(string name, IDictionary<string, object>? tags = null)
        {
            var activity = AppActivitySource.Instance.StartActivity(
                name,
                ActivityKind.Internal);

            if (activity != null && tags != null)
            {
                foreach (var tag in tags)
                    activity.SetTag(tag.Key, tag.Value);
            }

            return activity ?? DisposableAction.Empty;
        }
    }
}
