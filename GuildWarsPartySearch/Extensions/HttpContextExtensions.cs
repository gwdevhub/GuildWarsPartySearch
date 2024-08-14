using GuildWarsPartySearch.Server.Models;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Extensions;

public static class HttpContextExtensions
{
    private const string PermissionLevelKey = "PermissionLevel";
    private const string ClientIPKey = "ClientIP";

    public static void SetPermissionLevel(this HttpContext context, PermissionLevel permissionLevel)
    {
        context.ThrowIfNull()
            .Items.Add(PermissionLevelKey, permissionLevel);
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
}
