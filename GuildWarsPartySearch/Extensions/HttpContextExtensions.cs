using GuildWarsPartySearch.Server.Models;
using Microsoft.CorrelationVector;
using Serilog.Context;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Extensions;

public static class HttpContextExtensions
{
    private const string CorrelationVectorKey = "CorrelationVector";
    private const string ApiKey = "ApiKey";
    private const string PermissionLevelKey = "PermissionLevel";
    private const string PermissionReasonKey = "PermissionReason";
    private const string ClientIPKey = "ClientIP";

    public static void SetPermissionLevel(this HttpContext context, PermissionLevel permissionLevel, string reason)
    {
        context.ThrowIfNull()
            .Items.Add(PermissionLevelKey, permissionLevel);
        context.Items.Add(PermissionReasonKey, reason);
    }

    public static PermissionLevel GetPermissionLevel(this HttpContext context)
    {
        context.ThrowIfNull();
        if (!context.Items.TryGetValue(PermissionLevelKey, out var level) ||
            level is not PermissionLevel permissionLevel)
        {
            return PermissionLevel.None;
        }

        return permissionLevel;
    }

    public static string? GetPermissionLevelReason(this HttpContext context)
    {
        context.ThrowIfNull();
        if (!context.Items.TryGetValue(PermissionReasonKey, out var reason) ||
            reason is not string reasonStr)
        {
            return default;
        }

        return reasonStr;
    }

    public static void SetClientIP(this HttpContext context, string ip)
    {
        context.ThrowIfNull()
            .Items.Add(ClientIPKey, ip);
    }

    public static string GetClientIP(this HttpContext context)
    {
        context.ThrowIfNull();
        if (!context.Items.TryGetValue(ClientIPKey, out var ip) ||
            ip is not string ipStr)
        {
            throw new InvalidOperationException("Unable to extract IP from context");
        }

        return ipStr;
    }

    public static void SetApiKey(this HttpContext context, string apiKey)
    {
        context.ThrowIfNull()
            .Items.Add(ApiKey, apiKey);
    }

    public static string GetApiKey(this HttpContext context)
    {
        context.ThrowIfNull();
        if (!context.Items.TryGetValue(ApiKey, out var apiKey) ||
            apiKey is not string apiKeyStr)
        {
            throw new InvalidOperationException("Unable to extract API Key from context");
        }

        return apiKeyStr;
    }

    public static void SetCorrelationVector(this HttpContext context, CorrelationVector cv)
    {
        context.ThrowIfNull()
            .Items.Add(CorrelationVectorKey, cv);
    }

    public static CorrelationVector GetCorrelationVector(this HttpContext context)
    {
        context.ThrowIfNull();
        if (!context.Items.TryGetValue(CorrelationVectorKey, out var cvVal) ||
            cvVal is not CorrelationVector cv)
        {
            throw new InvalidOperationException("Unable to extract API Key from context");
        }

        return cv;
    }
}
