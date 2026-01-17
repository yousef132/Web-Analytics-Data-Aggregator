using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Controllers;
using Serilog.Context;

namespace DataAggergator.Presentation.Middlewares
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // 1️⃣ Get or create correlation ID
            var correlationId = Guid.NewGuid();
            Correlation.Current.Value = correlationId;

            context.Response.Headers["X-Correlation-ID"] = correlationId.ToString();

            //var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Anonymous";

            var endpoint = context.GetEndpoint();
            var actionName = endpoint?.Metadata
                .GetMetadata<ControllerActionDescriptor>()?.ActionName ?? "UnknownAction";
            var controllerName = endpoint?.Metadata
                .GetMetadata<ControllerActionDescriptor>()?.ControllerName ?? "UnknownController";

            using (LogContext.PushProperty("CorrelationId", correlationId))
            //using (LogContext.PushProperty("UserId", userId))
            using (LogContext.PushProperty("ControllerName", controllerName))
            using (LogContext.PushProperty("ActionName", actionName))
            {
                await _next(context);
            }
        }
    }
}
