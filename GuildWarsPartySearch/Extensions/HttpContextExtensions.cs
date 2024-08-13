using GuildWarsPartySearch.Server.Models;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Extensions;

public static class HttpContextExtensions
{
    private const string PermissionLevelKey = "PermissionLevel";

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
}
