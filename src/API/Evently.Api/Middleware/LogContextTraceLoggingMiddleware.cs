using System.Diagnostics;
using Serilog.Context;

namespace Evently.Api.Middleware;

internal sealed class LogContextTraceLoggingMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        string traceId = Activity.Current?.TraceId.ToString();
        
        using (LogContext.PushProperty("TraceId", traceId))
        {
            return next(context);
        }
    }
}
