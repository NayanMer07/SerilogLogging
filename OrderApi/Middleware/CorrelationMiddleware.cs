using Serilog.Context;

public class CorrelationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        // Read from incoming header or generate new ID
        var correlationId = ctx.Request.Headers["X-Correlation-ID"]
            .FirstOrDefault() ?? Guid.NewGuid().ToString("N");

        // Push to ALL logs in this request scope
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("UserId",
                ctx.User?.Identity?.Name ?? "anonymous"))
        {
            ctx.Response.Headers["X-Correlation-ID"] = correlationId;
            await next(ctx);
        }
    }
}