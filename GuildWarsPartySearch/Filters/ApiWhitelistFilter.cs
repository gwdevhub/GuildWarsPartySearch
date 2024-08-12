using GuildWarsPartySearch.Server.Options;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Filters
{
    public class ApiWhitelistFilter : IAsyncActionFilter
    {
        private const string XApiKeyHeaderKey = "X-Api-Key";

        private readonly ApiWhitelistOptions apiWhitelistOptions;
        private readonly ILogger<ApiWhitelistFilter> logger;

        public ApiWhitelistFilter(
            IOptions<ApiWhitelistOptions> options,
            ILogger<ApiWhitelistFilter> logger)
        {
            this.apiWhitelistOptions = options.ThrowIfNull().Value.ThrowIfNull();
            this.logger = logger.ThrowIfNull();
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var address = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var scopedLogger = this.logger.CreateScopedLogger(nameof(this.OnActionExecutionAsync), address ?? string.Empty);
            scopedLogger.LogDebug($"Received request");
            if (context.HttpContext.Request.Headers.TryGetValue(XApiKeyHeaderKey, out var xApiKeyvalues))
            {
                scopedLogger.LogDebug($"X-Api-Key {string.Join(',', xApiKeyvalues.Select(s => s))}");
            }

            if (xApiKeyvalues.None(k => k == this.apiWhitelistOptions.Key))
            {
                context.Result = new ForbiddenResponseActionResult("Forbidden");
                return;
            }

            await next();
        }
    }

}
