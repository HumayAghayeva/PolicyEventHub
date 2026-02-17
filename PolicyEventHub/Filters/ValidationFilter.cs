using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using PolicyEventHub.Models.Api;
using PolicyEventHub.Extensions;

namespace PolicyEventHub.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.ModelState.IsValid)
            {
                var controller = (ControllerBase)context.Controller;
                var correlationId = controller.GetCorrelationId();

                var issues = context.ModelState
                    .Where(kvp => kvp.Value?.Errors.Any() == true)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToArray();

                var details = issues.Select(i => new ErrorDetail { Issue = i });

                var apiResponse = ApiResponse<EmptyData>.Failure(
                    code: "VALIDATION_ERROR",
                    message: "Bad Request",
                    details: details,
                    correlationId: correlationId);

                context.Result = new BadRequestObjectResult(apiResponse);
                return; // short-circuit pipeline
            }

            await next();
        }
    }
}
