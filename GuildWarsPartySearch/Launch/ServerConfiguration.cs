using GuildWarsPartySearch.Server.Endpoints;
using GuildWarsPartySearch.Server.Options;
using GuildWarsPartySearch.Server.Services.Database;
using GuildWarsPartySearch.Server.Services.Feed;
using GuildWarsPartySearch.Server.Services.Lifetime;
using GuildWarsPartySearch.Server.Services.Logging;
using GuildWarsPartySearch.Server.Services.Options;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MTSC.Common.Http;
using MTSC.ServerSide;
using MTSC.ServerSide.Handlers;
using Slim;
using System.Core.Extensions;
using System.Extensions;
using System.Logging;
using System.Runtime.CompilerServices;
using static MTSC.Common.Http.HttpMessage;

namespace GuildWarsPartySearch.Server.Launch;

public static class ServerConfiguration
{
    private const string ApiKeyHeader = "X-ApiKey";

    public static IServiceManager SetupServiceManager(this IServiceManager serviceManager)
    {
        serviceManager.ThrowIfNull();

        serviceManager.RegisterResolver(new LoggerResolver());
        serviceManager.RegisterOptionsResolver();
        serviceManager.RegisterLogWriter<ConsoleLogger>();
        serviceManager.RegisterOptionsManager<JsonOptionsManager>();

        return serviceManager;
    }

    public static IServiceCollection SetupServices(this IServiceCollection services)
    {
        services.ThrowIfNull();

        services.AddScoped<IServerLifetimeService, ServerLifetimeService>();
        services.AddScoped<IPartySearchDatabase, InMemoryPartySearchDatabase>();
        services.AddScoped<IPartySearchService, PartySearchService>();
        services.AddSingleton<ILiveFeedService, LiveFeedService>();
        return services;
    }

    public static WebsocketRoutingHandler SetupRoutes(this WebsocketRoutingHandler websocketRoutingHandler)
    {
        websocketRoutingHandler.ThrowIfNull();
        websocketRoutingHandler
            .AddRoute<LiveFeed>("party-search/live-feed")
            .AddRoute<PostPartySearch>("party-search/update", FilterUpdateMessages);
        return websocketRoutingHandler;
    }

    private static RouteEnablerResponse FilterUpdateMessages(MTSC.ServerSide.Server server, HttpRequest req, ClientData clientData)
    {
        var serverOptions = server.ServiceManager.GetRequiredService<IOptions<ServerOptions>>();
        if (serverOptions.Value.ApiKey!.IsNullOrWhiteSpace())
        {
            return RouteEnablerResponse.Error(Forbidden403);
        }

        if (!req.Headers.ContainsHeader(ApiKeyHeader))
        {
            return RouteEnablerResponse.Error(Forbidden403);
        }

        var apiKey = req.Headers[ApiKeyHeader];
        if (apiKey != serverOptions.Value.ApiKey)
        {
            return RouteEnablerResponse.Error(Forbidden403);
        }
        
        return RouteEnablerResponse.Accept;
    }

    private static HttpResponse Forbidden403 => new()
    {
        StatusCode = StatusCodes.Forbidden
    };
}
