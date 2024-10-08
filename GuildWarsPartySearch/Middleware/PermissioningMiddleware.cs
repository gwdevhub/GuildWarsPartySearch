﻿using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Services.Permissions;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Middleware;

public sealed class PermissioningMiddleware : IMiddleware
{
    private const string EmptyApiKey = "-";
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
        var scopedLogger = this.logger.CreateScopedLogger(nameof(this.InvokeAsync), string.Empty);
        if (context.Request.Headers.TryGetValue(XApiKeyHeaderKey, out var xApiKeyvalues))
        {
            scopedLogger.LogDebug($"X-Api-Key {string.Join(',', xApiKeyvalues.Select(s => s))}");
        }

        if (xApiKeyvalues.FirstOrDefault() is not string apiKey)
        {
            context.SetPermissionLevel(Models.PermissionLevel.None, "No API Key found in X-Api-Key header");
            context.SetApiKey(EmptyApiKey);
            await next(context);
            return;
        }

        var permissionLevel = await this.permissionService.GetPermissionLevel(apiKey, context.RequestAborted);
        context.SetApiKey(apiKey);
        context.SetPermissionLevel(permissionLevel, $"Api key {apiKey} is associated with {permissionLevel} permission level");
        await next(context);
    }
}
