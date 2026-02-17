using PolicyEventHub.Framework.Exceptions;
using PolicyEventHub.Models.Api;

namespace PolicyEventHub.Middlewares
{
    public class DomainValidationExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DomainValidationExceptionMiddleware> _logger;

        public DomainValidationExceptionMiddleware(RequestDelegate next, ILogger<DomainValidationExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainValidationException dv)
            {
                _logger.LogWarning("Validation failed: {Message}", dv.Message);

                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";

                var correlationId = context.GetCorrelationId();

                var details = dv.Issues.Any()
                    ? dv.Issues.Select(i => new ErrorDetail { Issue = i }).ToArray()
                    : new[] { new ErrorDetail { Issue = "DomainValidationFailed" } };

                // Build the full response
                var response = new
                {
                    data = (object?)null,
                    meta = new
                    {
                        timeStamp = DateTime.UtcNow,
                        correlationId
                    },
                    error = new
                    {
                        code = "VALIDATION_ERROR",
                        message = "Bad Request",
                        details
                    }
                };

                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
