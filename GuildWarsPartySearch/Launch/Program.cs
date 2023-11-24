// See https://aka.ms/new-console-template for more information
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
                new HttpRoutingHandler()
                    .SetupRoutes()
                    .WithReturn404OnNotFound(true)
                    .WithReturn500OnUnhandledException(true))
            .AddServerUsageMonitor(new TickrateEnforcer() { TicksPerSecond = 240, Silent = true })
            .SetScheduler(new TaskAwaiterScheduler())
            .WithLoggingMessageContents(false);

        await server.RunAsync(CancellationTokenSource.Token);
    }
}