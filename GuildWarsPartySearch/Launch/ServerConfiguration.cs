using GuildWarsPartySearch.Server.Endpoints;
using GuildWarsPartySearch.Server.Extensions;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Content;
using GuildWarsPartySearch.Server.Services.Database;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.Lifetime;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.Extensions.Options;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Launch;

public static class ServerConfiguration
{
    private const string ApiKeyHeader = "X-ApiKey";

    public static WebApplicationBuilder SetupHostedServices(this WebApplicationBuilder builder)
    {
        builder.ThrowIfNull()
            .Services
            .AddHostedService<ContentRetrievalService>();

        return builder;
    }

    public static IConfigurationBuilder SetupConfiguration(this IConfigurationBuilder builder)
    {
        builder.ThrowIfNull()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Config.json");

        return builder;
    }

    public static ILoggingBuilder SetupLogging(this ILoggingBuilder builder)
    {
        builder.ThrowIfNull()
            .ClearProviders()
            .AddConsole();

        return builder;
    }

    public static WebApplicationBuilder SetupOptions(this WebApplicationBuilder builder)
    {
        builder.ThrowIfNull()
            .Services.Configure<EnvironmentOptions>(builder.Configuration.GetSection(nameof(EnvironmentOptions)))
                     .Configure<ContentOptions>(builder.Configuration.GetSection(nameof(ContentOptions)))
                     .Configure<StorageAccountOptions>(builder.Configuration.GetSection(nameof(StorageAccountOptions)))
                     .Configure<ServerOptions>(builder.Configuration.GetSection(nameof(ServerOptions)));

        return builder;
    }

    public static IServiceCollection SetupServices(this IServiceCollection services)
    {
        services.ThrowIfNull();

        services.AddScoped<IServerLifetimeService, ServerLifetimeService>();
        services.AddScoped<IPartySearchDatabase, TableStorageDatabase>();
        services.AddScoped<IPartySearchService, PartySearchService>();
        services.AddSingleton<ILiveFeedService, LiveFeedService>();
        return services;
    }

    public static WebApplication SetupRoutes(this WebApplication app)
    {
        app.ThrowIfNull()
            .MapWebSocket<LiveFeed>("party-search/live-feed")
            .MapWebSocket<PostPartySearch>("party-search/update", FilterUpdateMessages);

        return app;
    }

    private static Task<bool> FilterUpdateMessages(HttpContext context)
    {
        var serverOptions = context.RequestServices.GetRequiredService<IOptions<ServerOptions>>();
        if (serverOptions.Value.ApiKey!.IsNullOrWhiteSpace())
        {
            return Task.FromResult(false);
        }

        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var value) ||
            value.FirstOrDefault() is not string headerValue ||
            headerValue != serverOptions.Value.ApiKey)
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}
