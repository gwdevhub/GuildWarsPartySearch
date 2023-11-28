using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Telemetry;

public sealed class WebSocketTelemetryProcessor : ITelemetryProcessor
{
    private readonly ITelemetryProcessor next;

    public WebSocketTelemetryProcessor(ITelemetryProcessor next)
    {
        this.next = next.ThrowIfNull();
    }

    public void Process(ITelemetry item)
    {
        if (item is not RequestTelemetry request)
        {
            return;
        }

        if (request.Url.Scheme is "ws" or "wss")
        {
            request.Duration = TimeSpan.Zero;
        }

        this.next.Process(item);
    }
}
