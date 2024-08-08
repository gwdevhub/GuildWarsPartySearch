using Microsoft.Extensions.Hosting;

namespace GuildWarsPartySearch.Tests.Infra;

internal class TestHostLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted { get; }
    public CancellationToken ApplicationStopped { get; }
    public CancellationToken ApplicationStopping { get; }

    public void StopApplication()
    {
        throw new NotImplementedException();
    }
}
