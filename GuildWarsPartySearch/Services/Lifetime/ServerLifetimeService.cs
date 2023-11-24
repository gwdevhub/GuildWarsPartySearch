namespace GuildWarsPartySearch.Server.Services.Lifetime;

public sealed class ServerLifetimeService : IServerLifetimeService
{
    public void Kill()
    {
        Launch.Program.CancellationTokenSource.Cancel();
    }
}
