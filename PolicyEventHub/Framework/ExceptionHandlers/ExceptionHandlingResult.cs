using PolicyEventHub.Models.Api;

namespace PolicyEventHub.Framework.ExceptionHandlers
{
    public sealed class ExceptionHandlingResult
    {
        public int StatusCode { get; }
        public ApiResponse<EmptyData> Response { get; }

        public ExceptionHandlingResult(int statusCode, ApiResponse<EmptyData> response)
        {
            StatusCode = statusCode;
            Response = response;
        }
    }

    public interface IExceptionHandler
    {
        Task<ExceptionHandlingResult?> HandleAsync(Exception exception, HttpContext context);
    }

}
