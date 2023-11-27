using GuildWarsPartySearch.Server.Options;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Filters;

public sealed class ApiKeyProtected : IActionFilter
{
    private const string ApiKeyHeader = "X-ApiKey";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var serverOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ServerOptions>>();
        if (serverOptions.Value.ApiKey!.IsNullOrWhiteSpace())
        {
            context.Result = new ForbiddenResponseActionResult();
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var value) ||
            value.FirstOrDefault() is not string headerValue ||
            headerValue != serverOptions.Value.ApiKey)
        {
            context.Result = new ForbiddenResponseActionResult();
            return;
        }

        return;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
