using GuildWarsPartySearch.Server.Extensions;
using Serilog.Context;

namespace GuildWarsPartySearch.Server.Middleware;

public sealed class LoggingEnrichmentMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var ip = context.GetClientIP();
        var apiKey = context.GetApiKey();
        var cv = context.GetCorrelationVector();
        LogContext.PushProperty("ApiKey", apiKey);
        LogContext.PushProperty("ClientIP", ip);
        LogContext.PushProperty("CorrelationVector", cv);
        return next(context);
    }
}
