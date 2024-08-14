using GuildWarsPartySearch.Server.Extensions;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Middleware;

public sealed class HeaderLoggingMiddleware : IMiddleware
{
    private readonly ILogger<HeaderLoggingMiddleware> logger;

    public HeaderLoggingMiddleware(
        ILogger<HeaderLoggingMiddleware> logger)
    {
        this.logger = logger.ThrowIfNull();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var clientIp = context.Request.HttpContext.GetClientIP();
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.InvokeAsync), clientIp);
        scopedLogger.LogDebug(string.Join("\n", context.Request.Headers.Select(kvp => $"{kvp.Key}: {string.Join(",", kvp.Value.OfType<string>())}")));
        await next(context);
    }
}
