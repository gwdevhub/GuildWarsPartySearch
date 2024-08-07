// See https://aka.ms/new-console-template for more information
using AspNetCoreRateLimit;
using GuildWarsPartySearch.Server.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace GuildWarsPartySearch.Server.Launch;

public class Program
{
    public static readonly CancellationTokenSource CancellationTokenSource = new();

    private static async Task Main()
    {
        var config = new ConfigurationBuilder()
            .SetupConfiguration()
            .Build();

        var jsonOptions = new JsonSerializerOptions();
        jsonOptions.Converters.SetupConverters();
        jsonOptions.AllowTrailingCommas = true;

        var builder = WebApplication.CreateBuilder()
            .SetupOptions()
            .SetupHostedServices();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Guild Wars Party Search API", Version = "v1" });
            c.DocumentFilter<WebSocketEndpointsDocumentFilter>();
            c.EnableAnnotations();
        });
        builder.Services.AddSingleton(jsonOptions);
        builder.Logging.SetupLogging();
        builder.Services.SetupServices();
        builder.Services.AddControllers();
        builder.Configuration.AddConfiguration(config);
        builder.WebHost.ConfigureKestrel(kestrelOptions =>
        {
            kestrelOptions.Listen(IPAddress.Any, 443, listenOptions =>
            {
                var serverOptions = builder.Configuration.GetRequiredSection(nameof(ServerOptions)).Get<ServerOptions>();
                var certificateBytes = Convert.FromBase64String(serverOptions?.Certificate!);
                var certificate = new X509Certificate2(certificateBytes, serverOptions?.CertificatePassword);
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
        app.UseSwagger()
           .UseIpRateLimiting()
           .UseWebSockets()
           .UseRouting()
           .UseEndpoints(endpoints =>
           {
               endpoints.MapControllers();
           })
           .UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "Guild Wars Party Search API"))
           .UseStaticFiles(new StaticFileOptions
           {
               FileProvider = new PhysicalFileProvider(contentDirectory.FullName)
           });
        app.MapGet("/", context =>
            {
                context.Response.Redirect("/index.html");
                return Task.CompletedTask;
            });

        app.SetupRoutes();

        await app.RunAsync();
    }
}