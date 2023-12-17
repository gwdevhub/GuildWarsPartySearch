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
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ApiKeyProtected>>();
        logger.LogDebug("Verifying API key");
        if (serverOptions.Value.ApiKey!.IsNullOrWhiteSpace())
        {
            logger.LogCritical("API key is not configured");
            context.Result = new ForbiddenResponseActionResult("API Key is not configured");
            return;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeader, out var value))
        {
            logger.LogDebug("Request does not have an API key");
            context.Result = new ForbiddenResponseActionResult($"{ApiKeyHeader} header not found");
            return;
        }

        if (value.FirstOrDefault() is not string headerValue)
        {
            logger.LogDebug("Provided API key is not a string");
            context.Result = new ForbiddenResponseActionResult($"{ApiKeyHeader} header value is invalid");
            return;
        }

        if (headerValue != serverOptions.Value.ApiKey)
        {
            logger.LogDebug("API key does not match the configured key");
            context.Result = new ForbiddenResponseActionResult($"{ApiKeyHeader} header value is incorrect");
            return;
        }

        logger.LogDebug($"Verified API key {headerValue} = {serverOptions.Value.ApiKey}");
        return;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
