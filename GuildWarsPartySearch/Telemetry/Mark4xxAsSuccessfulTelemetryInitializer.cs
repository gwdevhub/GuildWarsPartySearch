using GuildWarsPartySearch.Server.Options;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Telemetry;

public sealed class Mark4xxAsSuccessfulTelemetryInitializer : ITelemetryInitializer
{
    private readonly TelemetryOptions telemetryOptions;

    public Mark4xxAsSuccessfulTelemetryInitializer(
        IOptions<TelemetryOptions> options)
    {
        this.telemetryOptions = options.ThrowIfNull().Value;
    }

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not RequestTelemetry requestTelemetry)
        {
            return;
        }

        if (!int.TryParse(requestTelemetry.ResponseCode, out var statusCode))
        {
            return;
        }

        if (this.telemetryOptions.SuccessfulStatusCodes?.Contains(statusCode) is true)
        {
            requestTelemetry.Success = true;
        }

        return;
    }
}
