using MTSC.Common.Http.Attributes;
using MTSC.Common.Http;
using MTSC.Exceptions;

namespace GuildWarsPartySearch.Server.Filters;

public sealed class ReturnBadRequestOnDataBindingFailureAttribute : RouteFilterAttribute
{
    public override RouteFilterExceptionHandlingResponse HandleException(RouteContext routeFilterContext, Exception exception)
    {
        if (exception is DataBindingException)
        {
            return BadRequest400("Bad request");
        }
        else
        {
            return base.HandleException(routeFilterContext, exception);
        }
    }

    private static RouteFilterExceptionHandlingResponse BadRequest400(string message) => RouteFilterExceptionHandlingResponse.Handled(
        new HttpResponse
        {
            StatusCode = HttpMessage.StatusCodes.BadRequest,
            BodyString = message
        });
}
