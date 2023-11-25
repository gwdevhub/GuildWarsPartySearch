// See https://aka.ms/new-console-template for more information
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.ServerHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MTSC.ServerSide.Handlers;
using MTSC.ServerSide.Schedulers;
using MTSC.ServerSide.UsageMonitors;

namespace GuildWarsPartySearch.Server.Launch;

public class Program
{
    public static CancellationTokenSource CancellationTokenSource = new();

    private static async Task Main()
    {
        var server = new MTSC.ServerSide.Server(8080);
        server.ServiceCollection.SetupServices();
        server.ServiceManager.SetupServiceManager();
        server
            .AddHandler(
                new WebsocketRoutingHandler()
                    .SetupRoutes()
                    .WithHeartbeatEnabled(true)
                    .WithHeartbeatFrequency(server.ServiceManager.GetRequiredService<IOptions<ServerOptions>>().Value.HeartbeatFrequency ?? TimeSpan.FromSeconds(5)))
            .AddHandler(new ConnectionMonitorHandler())
            .AddHandler(new StartupHandler())
            .AddServerUsageMonitor(new TickrateEnforcer() { TicksPerSecond = 60, Silent = true })
            .SetScheduler(new TaskAwaiterScheduler())
            .WithLoggingMessageContents(false);

        await server.RunAsync(CancellationTokenSource.Token);
    }
}