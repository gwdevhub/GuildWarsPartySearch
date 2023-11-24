using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.Lifetime;
using Microsoft.Extensions.Logging;
using MTSC.Common.Http;
using MTSC.Common.Http.RoutingModules;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[SimpleStringTokenFilter]
public sealed class Kill : HttpRouteBase<None>
{
    private IServerLifetimeService serverLifetimeService;
    private ILogger<Kill> logger;

    public Kill(
        IServerLifetimeService serverLifetimeService,
        ILogger<Kill> logger)
    {
        this.serverLifetimeService = serverLifetimeService.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public override Task<HttpResponse> HandleRequest(None _)
    {
        this.serverLifetimeService.Kill();
        return Task.FromResult(Success200);
    }

    private static HttpResponse Success200 => new()
    {
        StatusCode = HttpMessage.StatusCodes.OK,
        BodyString = "Stopping server"
    };
}
