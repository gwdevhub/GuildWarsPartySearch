using Microsoft.AspNetCore.Mvc;

namespace GuildWarsPartySearch.Server.Filters;

public class ForbiddenResponseActionResult : IActionResult
{
    public Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    }
}
