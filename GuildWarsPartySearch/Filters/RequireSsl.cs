using Microsoft.AspNetCore.Mvc.Filters;

namespace GuildWarsPartySearch.Server.Filters;

public sealed class RequireSsl : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Scheme is not "https" or "wss")
        {
            context.Result = new ForbiddenResponseActionResult($"This endpoint requires SSL communication");
        }
    }
}
