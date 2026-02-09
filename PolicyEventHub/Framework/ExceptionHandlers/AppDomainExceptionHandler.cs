using PolicyEventHub.Applications.Domain.Exceptions;
using PolicyEventHub.Framework.Exceptions;
using PolicyEventHub.Models.Api;

namespace PolicyEventHub.Framework.ExceptionHandlers
{
    public class AppDomainExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<DomainValidationExceptionHandler> _logger;

        public AppDomainExceptionHandler(ILogger<DomainValidationExceptionHandler> logger)
        {
            _logger = logger;
        }

        public Task<ExceptionHandlingResult?> HandleAsync(Exception exception, HttpContext context)
        {
            if (exception is not AppDomainException dv)
                return Task.FromResult<ExceptionHandlingResult?>(null);

            var correlationId = context.GetCorrelationId();

            _logger.LogWarning(
                exception,
                "App Domain error: {Message} | Issues: {Issues} | CorrelationId: {CorrelationId}",
                dv.Message,
                string.Join(",", dv.Issues),
                correlationId);

            var details = dv.Issues.Any()
                ? dv.Issues.Select(i => new ErrorDetail { Issue = i }).ToArray()
                : new[] { new ErrorDetail { Issue = "APP_ERROR" } };

            var apiResponse = ApiResponse<EmptyData>.Failure(
                code: dv.ErrorCode,
                message: dv.Message,
                details: details,
                correlationId: correlationId);

            var result = new ExceptionHandlingResult(
                StatusCodes.Status400BadRequest,
                apiResponse);

            return Task.FromResult<ExceptionHandlingResult?>(result);
        }
    }
}
