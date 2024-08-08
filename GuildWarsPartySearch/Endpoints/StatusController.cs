using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.BotStatus;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[Route("status")]
public class StatusController : Controller
{
    private readonly IBotStatusService botStatusService;

    public StatusController(
        IBotStatusService botStatusService)
    {
        this.botStatusService = botStatusService.ThrowIfNull();
    }

    [HttpGet("bots")]
    [ServiceFilter<IpWhitelistFilter>]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [SwaggerOperation(Description = $"Protected by *IP whitelisting*.\r\n\r\n")]
    public async Task<IActionResult> GetBotStatus()
    {
        return this.Ok(await this.botStatusService.GetBots());
    }

    [HttpGet("map-activity")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetActiveMaps()
    {
        var bots = await this.botStatusService.GetBots();
        return this.Ok(bots.Select(b => new MapActivity
        {
            District = b.District,
            MapId = b.Map?.Id ?? -1,
            LastUpdate = b.LastSeen ?? DateTime.MinValue
        }));
    }
}
