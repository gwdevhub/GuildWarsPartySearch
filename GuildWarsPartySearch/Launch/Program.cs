// See https://aka.ms/new-console-template for more information
using GuildWarsPartySearch.Server.HttpModules;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.ServerHandlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        var server = new MTSC.ServerSide.Server(443);
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
            .AddHandler(new ContentManagementHandler())
            .AddHandler(new HttpHandler()
                .AddHttpModule(new ContentModule()))
            .AddServerUsageMonitor(new TickrateEnforcer() { TicksPerSecond = 60, Silent = true })
            .SetScheduler(new TaskAwaiterScheduler())
            .WithLoggingMessageContents(false);

        var verificationServer = new MTSC.ServerSide.Server(80);
        verificationServer.ServiceCollection.AddScoped(_ => server.ServiceManager.GetRequiredService<ILogger<MTSC.ServerSide.Server>>());
        verificationServer.ServiceCollection.AddScoped(_ => server.ServiceManager.GetRequiredService<ILogger<ContentModule>>());
        verificationServer.ServiceCollection.AddScoped(_ => server.ServiceManager.GetRequiredService<IOptions<ContentOptions>>());
        verificationServer.AddHandler(new HttpHandler()
            .AddHttpModule(new ContentModule()))
            .AddServerUsageMonitor(new TickrateEnforcer { TicksPerSecond = 10, Silent = true })
            .SetScheduler(new TaskAwaiterScheduler())
            .WithLoggingMessageContents(false);

        //Ugly hack to copy the setup from the https server onto the http server
        var serverTask = server.RunAsync(CancellationTokenSource.Token);
        var verificationServerTask = verificationServer.RunAsync(CancellationTokenSource.Token);

        await Task.WhenAll(
            serverTask,
            verificationServerTask);
    }
}