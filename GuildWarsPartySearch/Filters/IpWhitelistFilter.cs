using GuildWarsPartySearch.Server.Services.Database;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Filters
{
    public class IpWhitelistFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var db = context.HttpContext.RequestServices.GetRequiredService<IIpWhitelistDatabase>();
            var ipAddresses = await db.GetWhitelistedAddresses(context.HttpContext.RequestAborted);
            var address = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            if (ipAddresses.None(whiteListed => whiteListed == address))
            {
                context.Result = new ForbiddenResponseActionResult("Forbidden");
                return;
            }

            await next();
        }
    }
}
