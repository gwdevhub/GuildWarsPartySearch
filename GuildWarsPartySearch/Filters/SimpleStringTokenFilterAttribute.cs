using Microsoft.Extensions.Logging;
using MTSC.Common.Http;
using MTSC.Common.Http.Attributes;
using Slim.Attributes;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Filters;

/// <summary>
/// Super stupid filter in place.
/// TODO: Change asap to a proper authentication scheme such as MTLS
/// </summary>
public sealed class SimpleStringTokenFilterAttribute : RouteFilterAttribute
{
    public static string StringTokenHeader = "X-Token";
    private static string StringTokenValue = "1234";

    private readonly ILogger<SimpleStringTokenFilterAttribute> logger;

    /// <summary>
    /// Need to keep this constructor so that this class can be used as an attribute.
    /// This constructor will never be called by the DI engine during normal usage.
    /// </summary>
    [DoNotInject]
    public SimpleStringTokenFilterAttribute()
    {
        this.logger = default!;
    }

    public SimpleStringTokenFilterAttribute(
        ILogger<SimpleStringTokenFilterAttribute> logger)
    {
        this.logger = logger.ThrowIfNull();
    }

    public override RouteEnablerResponse HandleRequest(RouteContext routeContext)
    {
        if (!routeContext.HttpRequest.Headers.ContainsHeader(StringTokenHeader))
        {
            return RouteEnablerResponse.Error(MissingToken403);
        }

        var token = routeContext.HttpRequest.Headers[StringTokenHeader];
        if (token != StringTokenValue)
        {
            return RouteEnablerResponse.Error(InvalidToken403);
        }

        return RouteEnablerResponse.Accept;
    }

    private static HttpResponse MissingToken403 => new()
    {
        StatusCode = HttpMessage.StatusCodes.Forbidden,
        BodyString = "Missing Token"
    };

    private static HttpResponse InvalidToken403 => new()
    {
        StatusCode = HttpMessage.StatusCodes.Forbidden,
        BodyString = "Invalid Token"
    };
}
