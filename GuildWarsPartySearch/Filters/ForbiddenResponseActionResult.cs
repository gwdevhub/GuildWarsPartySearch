using Microsoft.AspNetCore.Mvc;

namespace GuildWarsPartySearch.Server.Filters;

public class ForbiddenResponseActionResult : IActionResult
{
    private readonly string reason;

    public ForbiddenResponseActionResult(string reason)
    {
        this.reason = reason;
    }

    public Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return context.HttpContext.Response.WriteAsync(reason, context.HttpContext.RequestAborted);
    }
}
