using PolicyEventHub.Framework.Constants;

namespace PolicyEventHub.Framework.Middleware
{
    public class CorrelationIdMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<CorrelationIdMiddleware> _logger;

            public CorrelationIdMiddleware(
                RequestDelegate next,
                ILogger<CorrelationIdMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task Invoke(HttpContext context)
            {
                string correlationId;

                if (context.Request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out var header) &&
                    !string.IsNullOrWhiteSpace(header))
                {
                    correlationId = header.ToString();
                }
                else
                {
                    correlationId = Guid.NewGuid().ToString("D");
                }

                context.Items[CorrelationIdConstants.ItemName] = correlationId;
                context.TraceIdentifier = correlationId;

                context.Response.OnStarting(() =>
                {
                    context.Response.Headers[CorrelationIdConstants.HeaderName] = correlationId;
                    return Task.CompletedTask;
                });

                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId
                }))
                {
                    await _next(context);
                }
            }
        }
    }