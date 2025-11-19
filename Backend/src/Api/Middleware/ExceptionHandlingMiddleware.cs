using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            logger.LogError(ex, "Unhandled exception with traceId {TraceId}", traceId);

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var payload = JsonSerializer.Serialize(new
            {
                message = "Ha ocurrido un error interno.",
                traceId
            });

            await context.Response.WriteAsync(payload);
        }
    }
}
