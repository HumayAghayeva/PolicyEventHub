using Microsoft.AspNetCore.HttpLogging;
using PolicyEventHub.Framework.Middleware;
using PolicyEventHub.Middlewares;

namespace PolicyEventHub.Extensions
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseDomainValidationExceptionHandling(this IApplicationBuilder app)
         => app.UseMiddleware<DomainValidationExceptionMiddleware>();

        public static IApplicationBuilder UseGlobalErrorHandling(this IApplicationBuilder app)
            => app.UseMiddleware<ErrorHandlingMiddleware>();

        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
            => app.UseMiddleware<CorrelationIdMiddleware>();

        public static IServiceCollection AddHttpLoggingOptions(
            this IServiceCollection services,
            Action<HttpLoggingOptions>? configure = null)
        {
            if (configure != null)
            {
                services.Configure(configure);
            }
            else
            {
                services.Configure<HttpLoggingOptions>(_ => { });
            }

            return services;
        }

        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder app)
            => app.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}
