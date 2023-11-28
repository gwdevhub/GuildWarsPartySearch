using Microsoft.AspNetCore.Mvc.Filters;

namespace GuildWarsPartySearch.Server.Filters;

public class UserAgentRequired : IActionFilter
{
    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Headers.UserAgent.FirstOrDefault() is not string userAgent)
        {
            context.Result = new ForbiddenResponseActionResult("Missing user agent");
            return;
        }
    }
}
