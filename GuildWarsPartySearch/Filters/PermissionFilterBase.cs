using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GuildWarsPartySearch.Server.Filters;

public abstract class PermissionFilterBase : IAsyncActionFilter
{
    public abstract PermissionLevel PermissionLevel { get; }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var permissionLevel = context.HttpContext.GetPermissionLevel();
        if (permissionLevel < this.PermissionLevel)
        {
            var permissionReason = context.HttpContext.GetPermissionLevelReason();
            context.Result = new ForbiddenResponseActionResult($"Insufficient permissions. Expected permission level: {this.PermissionLevel}. Client permission level: {permissionLevel}. Reason: {permissionReason}");
            return;
        }

        await next();
    }
}
