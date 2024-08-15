using GuildWarsPartySearch.Server.Extensions;

namespace GuildWarsPartySearch.Server.Middleware;

public sealed class IPExtractingMiddleware : IMiddleware
{
    private const string XForwardedForHeaderKey = "X-Forwarded-For";
    private const string CFConnectingIPHeaderKey = "CF-Connecting-IP";

    public IPExtractingMiddleware()
    {
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var address = context.Connection.RemoteIpAddress?.ToString();
        context.Request.Headers.TryGetValue(CFConnectingIPHeaderKey, out var cfConnectingIpValues);
        if (cfConnectingIpValues.FirstOrDefault() is string xCfIpAddress)
        {
            context.SetClientIP(xCfIpAddress);
            await next(context);
            return;
        }

        context.Request.Headers.TryGetValue(XForwardedForHeaderKey, out var xForwardedForValues);
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
