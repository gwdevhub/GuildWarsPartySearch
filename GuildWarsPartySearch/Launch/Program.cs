// See https://aka.ms/new-console-template for more information
using GuildWarsPartySearch.Server.Endpoints;
using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Options;
using Microsoft.Extensions.FileProviders;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace GuildWarsPartySearch.Server.Launch;

public class Program
{
    public static CancellationTokenSource CancellationTokenSource = new();

    private static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .SetupConfiguration()
            .Build();

        var builder = WebApplication.CreateBuilder()
            .SetupOptions()
            .SetupHostedServices();
        builder.Logging.SetupLogging();
        builder.Services.SetupServices();
        builder.Configuration.AddConfiguration(config);
        builder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
            kestrelOptions.Listen(IPAddress.Any, 443, listenOptions =>
            {
                var serverOptions = builder.Configuration.GetRequiredSection(nameof(ServerOptions)).Get<ServerOptions>();
                var certificateBytes = Convert.FromBase64String(serverOptions?.Certificate!);
                var certificate = new X509Certificate2(certificateBytes);
                listenOptions.UseHttps(certificate);
            });

            kestrelOptions.Listen(IPAddress.Any, 80, listenOptions =>
            {
            });
        });

        var contentOptions = builder.Configuration.GetRequiredSection(nameof(ContentOptions)).Get<ContentOptions>()!;
        var contentDirectory = new DirectoryInfo(contentOptions.StagingFolder);
        if (!contentDirectory.Exists)
        {
            contentDirectory.Create();
        }

        var app = builder.Build();
        app.UseWebSockets()
           .UseStaticFiles(new StaticFileOptions
           {
               FileProvider = new PhysicalFileProvider(contentDirectory.FullName)
           });
        app.MapGet("/", context =>
        {
            context.Response.Redirect("/index.html");
            return Task.CompletedTask;
        });
        app.MapWebSocket<PostPartySearch>("party-search/update");
        app.MapWebSocket<LiveFeed>("party-search/live-feed");

        await app.RunAsync();
    }
}