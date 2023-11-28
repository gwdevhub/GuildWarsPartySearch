using GuildWarsPartySearch.Server.Options;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Filters;

public sealed class ApiKeyProtected : IActionFilter
{
    public const string ApiKeyHeader = "X-ApiKey";

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var serverOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ServerOptions>>();
        if (serverOptions.Value.ApiKey!.IsNullOrWhiteSpace())
        {
            context.Result = new ForbiddenResponseActionResult("API Key is not configured");
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var value))
        {
            context.Result = new ForbiddenResponseActionResult($"{ApiKeyHeader} header not found");
            return;
        }

        if (value.FirstOrDefault() is not string headerValue)
        {
            context.Result = new ForbiddenResponseActionResult($"{ApiKeyHeader} header value is invalid");
            return;
        }

        if (headerValue != serverOptions.Value.ApiKey)
        {
            context.Result = new ForbiddenResponseActionResult($"{ApiKeyHeader} header value is incorrect");
            return;
        }

        return;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
