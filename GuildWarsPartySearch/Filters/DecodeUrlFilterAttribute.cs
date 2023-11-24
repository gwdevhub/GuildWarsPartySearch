using MTSC.Common.Http;
using MTSC.Common.Http.Attributes;

namespace GuildWarsPartySearch.Server.Filters;

public sealed class DecodeUrlFilterAttribute : RouteFilterAttribute
{
    public override RouteEnablerResponse HandleRequest(RouteContext routeContext)
    {
        routeContext.HttpRequest.RequestURI = ReplaceUrlEncodings(routeContext.HttpRequest.RequestURI);

        foreach(var valueTuple in routeContext.UrlValues)
        {
            routeContext.UrlValues[valueTuple.Key] = ReplaceUrlEncodings(valueTuple.Value);
        }

        return RouteEnablerResponse.Accept;
    }

    private static string ReplaceUrlEncodings(string value)
    {
        return value.Replace("%20", " ")
            .Replace("%21", "!")
            .Replace("%22", "\"")
            .Replace("%23", "#")
            .Replace("%26", "&")
            .Replace("%27", "'");
    }
}
