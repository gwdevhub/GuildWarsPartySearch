using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Services.Permissions;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Middleware;

public sealed class PermissioningMiddleware : IMiddleware
{
    private const string XApiKeyHeaderKey = "X-Api-Key";

    private readonly IPermissionService permissionService;
    private readonly ILogger<PermissioningMiddleware> logger;

    public PermissioningMiddleware(
        IPermissionService permissionService,
        ILogger<PermissioningMiddleware> logger)
    {
        this.permissionService = permissionService.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var address = context.Request.HttpContext.GetClientIP();
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.InvokeAsync), address ?? string.Empty);
        if (context.Request.Headers.TryGetValue(XApiKeyHeaderKey, out var xApiKeyvalues))
        {
            scopedLogger.LogDebug($"X-Api-Key {string.Join(',', xApiKeyvalues.Select(s => s))}");
        }

        if (xApiKeyvalues.FirstOrDefault() is not string apiKey)
        {
            context.SetPermissionLevel(Models.PermissionLevel.None);
            await next(context);
            return;
        }

        var permissionLevel = await this.permissionService.GetPermissionLevel(apiKey, context.RequestAborted);
        context.SetPermissionLevel(permissionLevel);
        await next(context);
    }
}
