using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using PolicyEventHub.Extensions;
using PolicyEventHub.Framework.ExceptionHandlers;
using PolicyEventHub.Models.Api;
using System.Text.Json;

namespace PolicyEventHub.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly IExceptionHandler[] _handlers;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IOptions<JsonOptions> jsonOptions,
            IEnumerable<IExceptionHandler> handlers)
        {
            _next = next;
            _logger = logger;
            _jsonOptions = jsonOptions.Value.SerializerOptions;
            _handlers = handlers.ToArray();
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogError(ex, "Unhandled exception after response started.");
                    throw;
                }

                ExceptionHandlingResult? result = null;

                foreach (var handler in _handlers)
                {
                    result = await handler.HandleAsync(ex, context);
                    if (result != null)
                        break;
                }

                if (result == null)
                {
                    // Fallback 500 – nobody handled it
                    var correlationId = context.GetCorrelationId();

                    _logger.LogError(
                        ex,
                        "Unhandled exception during request processing. CorrelationId: {CorrelationId}",
                        correlationId);

                    var apiResponse = ApiResponse<EmptyData>.Failure(
                        code: "INTERNAL_ERROR",
                        message: ex.Message,
                        details: new[]
                        {
                     new ErrorDetail { Issue = "UnexpectedError" }
                        },
                        correlationId: correlationId);

                    result = new ExceptionHandlingResult(
                        StatusCodes.Status500InternalServerError,
                        apiResponse);
                }

                context.Response.Clear();
                context.Response.StatusCode = result.StatusCode;
                context.Response.ContentType = "application/json; charset=utf-8";

                var payload = JsonSerializer.Serialize(result.Response, _jsonOptions);
                await context.Response.WriteAsync(payload);
            }
        }
    }
}
