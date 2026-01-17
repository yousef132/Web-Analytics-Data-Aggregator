namespace DataAggergator.Presentation.Middlewares
{
    using System.Net;
    using System.Text.Json;
    using Microsoft.AspNetCore.Http;
    using Serilog;

    public class ExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); 
            }
            catch (Exception ex)
            {
                // Log based on exception type
                switch (ex)
                {
                    case KeyNotFoundException _:
                        Log.Warning(ex, "Resource not found: {Message}", ex.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;

                    case UnauthorizedAccessException _:
                        Log.Warning(ex, "Unauthorized access: {Message}", ex.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        break;

                    default:
                        Log.Error(ex, "Unhandled exception occurred: {Message}", ex.Message);
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }
                var correlationId = context.Response.Headers["X-Correlation-ID"].ToString();

                context.Response.ContentType = "application/json";
                var response = new
                {
                    Error = ex.Message,
                    StatusCode = context.Response.StatusCode,
                    CorrelationId = correlationId
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }

}
