using GuildWarsPartySearch.Server.Extensions;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Middleware;

public sealed class IPExtractingMiddleware : IMiddleware
{
    private const string XForwardedForHeaderKey = "X-Forwarded-For";
    private const string CFConnectingIPHeaderKey = "CF-Connecting-IP";

    private readonly ILogger<IPExtractingMiddleware> logger;

    public IPExtractingMiddleware(
        ILogger<IPExtractingMiddleware> logger)
    {
        this.logger = logger.ThrowIfNull();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var address = context.Connection.RemoteIpAddress?.ToString();
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.InvokeAsync), address ?? string.Empty);
        scopedLogger.LogDebug($"Received request");

        if (context.Request.Headers.TryGetValue(CFConnectingIPHeaderKey, out var cfConnectingIpValues))
        {
            scopedLogger.LogDebug($"CF-Connecting-IP {string.Join(',', cfConnectingIpValues.Select(s => s))}");
        }

        if (cfConnectingIpValues.FirstOrDefault() is string xCfIpAddress)
        {
            context.SetClientIP(xCfIpAddress);
            await next(context);
            return;
        }

        if (context.Request.Headers.TryGetValue(XForwardedForHeaderKey, out var xForwardedForValues))
        {
            scopedLogger.LogDebug($"X-Forwarded-For {string.Join(',', xForwardedForValues.Select(s => s))}");
        }

        if (xForwardedForValues.FirstOrDefault() is string xForwardedIpAddress)
        {
            context.SetClientIP(xForwardedIpAddress);
            await next(context);
            return;
        }

        context.SetClientIP(address ?? throw new InvalidOperationException("Unable to extract client IP address"));
        await next(context);
    }
}
