
using GuildWarsPartySearch.Server.Extensions;
using Microsoft.CorrelationVector;

namespace GuildWarsPartySearch.Server.Middleware;

public sealed class CorrelationVectorMiddleware : IMiddleware
{
    private const string CorrelationVectorHeader = "X-Correlation-Vector";

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var cv = new CorrelationVector();
        if (context.Items.TryGetValue(CorrelationVectorHeader, out var correlationVectorVal) &&
            correlationVectorVal is string correlationVectorStr)
        {
            cv = CorrelationVector.Parse(correlationVectorStr);
            cv.Increment();
        }

        context.SetCorrelationVector(cv);
        return next(context);
    }
}
