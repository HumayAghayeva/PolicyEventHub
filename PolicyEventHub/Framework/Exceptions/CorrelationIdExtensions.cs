using Microsoft.AspNetCore.Mvc;
using PolicyEventHub.Framework.Constants;

namespace PolicyEventHub.Framework.Exceptions
{
    public static class CorrelationIdExtensions
    {
        public static string GetCorrelationId(this HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            if (context.Items.TryGetValue(CorrelationIdConstants.ItemName, out var value) &&
                value is string id &&
                !string.IsNullOrWhiteSpace(id))
            {
                return id;
            }

            if (context.Request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out var header) &&
                !string.IsNullOrWhiteSpace(header))
            {
                return header.ToString();
            }

            return context.TraceIdentifier;
        }

        public static string GetCorrelationId(this ControllerBase controller)
            => controller.HttpContext.GetCorrelationId();
    }
}
