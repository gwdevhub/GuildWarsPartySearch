using GuildWarsPartySearch.Server.Endpoints;
using GuildWarsPartySearch.Server.Services.Database;
using GuildWarsPartySearch.Server.Services.Lifetime;
using GuildWarsPartySearch.Server.Services.Logging;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.Extensions.DependencyInjection;
using MTSC.ServerSide.Handlers;
using Slim;
using System.Core.Extensions;
using System.Extensions;
using System.Logging;
using static MTSC.Common.Http.HttpMessage;

namespace GuildWarsPartySearch.Server.Launch;

public static class ServerConfiguration
{
    public static IServiceManager SetupServiceManager(this IServiceManager serviceManager)
    {
        serviceManager.ThrowIfNull();

        serviceManager.RegisterResolver(new LoggerResolver());
        serviceManager.RegisterOptionsResolver();
        serviceManager.RegisterLogWriter<ConsoleLogger>();

        return serviceManager;
    }

    public static IServiceCollection SetupServices(this IServiceCollection services)
    {
        services.ThrowIfNull();

        services.AddScoped<IServerLifetimeService, ServerLifetimeService>();
        services.AddScoped<IPartySearchDatabase, InMemoryPartySearchDatabase>();
        services.AddScoped<IPartySearchService, PartySearchService>();
        return services;
    }

    public static HttpRoutingHandler SetupRoutes(this HttpRoutingHandler httpRoutingHandler)
    {
        httpRoutingHandler.ThrowIfNull();

        httpRoutingHandler
            .AddRoute<PostPartySearch>(HttpMethods.Post, "party-search")
            .AddRoute<GetPartySearch>(HttpMethods.Get, "party-search/{campaign}/{continent}/{region}/{map}/{district}");
        return httpRoutingHandler;
    }
}
