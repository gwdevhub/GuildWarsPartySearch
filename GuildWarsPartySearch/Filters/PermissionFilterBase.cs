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
            context.Result = new ForbiddenResponseActionResult("Forbidden");
            return;
        }

        await next();
    }
}
