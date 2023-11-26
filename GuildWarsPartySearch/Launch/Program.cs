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
        var httpsServer = new MTSC.ServerSide.Server(443);
        httpsServer.ServiceCollection.SetupServices();
        httpsServer.ServiceManager.SetupServiceManager();
        httpsServer
            .AddHandler(
                new WebsocketRoutingHandler()
                    .SetupRoutes()
                    .WithHeartbeatEnabled(true)
                    .WithHeartbeatFrequency(httpsServer.ServiceManager.GetRequiredService<IOptions<ServerOptions>>().Value.HeartbeatFrequency ?? TimeSpan.FromSeconds(5)))
            .AddHandler(new ConnectionMonitorHandler())
            .AddHandler(new StartupHandler())
            .AddHandler(new ContentManagementHandler())
            .AddHandler(new HttpHandler()
                .AddHttpModule(new ContentModule()))
            .AddServerUsageMonitor(new TickrateEnforcer() { TicksPerSecond = 60, Silent = true })
            .SetScheduler(new TaskAwaiterScheduler())
            .WithLoggingMessageContents(false);
        var serverOptions = httpsServer.ServiceManager.GetRequiredService<IOptions<ServerOptions>>();
        httpsServer.WithCertificate(serverOptions.Value.Certificate);
        httpsServer.WithClientCertificate(false);

        var httpServer = new MTSC.ServerSide.Server(80);
        httpServer.ServiceCollection.AddScoped(_ => httpsServer.ServiceManager.GetRequiredService<ILogger<MTSC.ServerSide.Server>>());
        httpServer.ServiceCollection.AddScoped(_ => httpsServer.ServiceManager.GetRequiredService<ILogger<ContentModule>>());
        httpServer.ServiceCollection.AddScoped(_ => httpsServer.ServiceManager.GetRequiredService<IOptions<ContentOptions>>());
        httpServer.AddHandler(new HttpHandler()
            .AddHttpModule(new ContentModule()))
            .AddServerUsageMonitor(new TickrateEnforcer { TicksPerSecond = 10, Silent = true })
            .SetScheduler(new TaskAwaiterScheduler())
            .WithLoggingMessageContents(false);

        var serverTask = httpsServer.RunAsync(CancellationTokenSource.Token);
        var verificationServerTask = httpServer.RunAsync(CancellationTokenSource.Token);

        await Task.WhenAll(
            serverTask,
            verificationServerTask);
    }
}