using GuildWarsPartySearch.Server.Services.Database;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Filters;

public class IpWhitelistFilter : IAsyncActionFilter
{
    private const string XForwardedForHeaderKey = "X-Forwarded-For";
    private const string CFConnectingIPHeaderKey = "CF-Connecting-IP";

    private readonly IIpWhitelistDatabase database;
    private readonly ILogger<IpWhitelistFilter> logger;

    public IpWhitelistFilter(
        IIpWhitelistDatabase database,
        ILogger<IpWhitelistFilter> logger)
    {
        this.database = database.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var address = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.OnActionExecutionAsync), address ?? string.Empty);
        var ipAddresses = await this.database.GetWhitelistedAddresses(context.HttpContext.RequestAborted);
        
        scopedLogger.LogDebug($"Received request");
        if (context.HttpContext.Request.Headers.TryGetValue(XForwardedForHeaderKey, out var xForwardedForValues))
        {
            scopedLogger.LogDebug($"X-Forwarded-For {string.Join(',', xForwardedForValues.Select(s => s))}");
        }

        if (context.HttpContext.Request.Headers.TryGetValue(CFConnectingIPHeaderKey, out var cfConnectingIpValues))
        {
            scopedLogger.LogDebug($"CF-Connecting-IP {string.Join(',', cfConnectingIpValues.Select(s => s))}");
        }

        if (ipAddresses.None(whiteListed => 
            whiteListed == address ||
            xForwardedForValues.Contains(whiteListed) ||
            cfConnectingIpValues.Contains(whiteListed)))
        {
            context.Result = new ForbiddenResponseActionResult("Forbidden");
            return;
        }

        await next();
    }
}
