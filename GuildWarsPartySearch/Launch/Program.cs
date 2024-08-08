// See https://aka.ms/new-console-template for more information
using GuildWarsPartySearch.Server.Options;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Net;
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
            var options = builder.Configuration.GetRequiredSection(nameof(ServerOptions)).Get<ServerOptions>();
            kestrelOptions.Listen(IPAddress.Any, options?.Port ?? 80, listenOptions =>
            {
            });
        });

        var contentOptions = builder.Configuration.GetRequiredSection(nameof(ContentOptions)).Get<ContentOptions>()!;
        var contentDirectory = new DirectoryInfo(
            Path.Combine(
                Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location) ?? throw new InvalidOperationException("Unable to get content staging folder"), contentOptions.StagingFolder));
        if (!contentDirectory.Exists)
        {
            contentDirectory.Create();
        }

        var app = builder.Build();
        app.UseSwagger()
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