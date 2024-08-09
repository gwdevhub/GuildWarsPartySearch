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

    [HttpGet("bot-activity/all")]
    [ServiceFilter<IpWhitelistFilter>]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [SwaggerOperation(Description = $"Protected by *IP whitelisting*.\r\n\r\n")]
    public async Task<IActionResult> GetAllBotActivity()
    {
        return this.Ok(await this.botStatusService.GetAllActivities(this.HttpContext.RequestAborted));
    }

    [HttpGet("bot-activity/bots/{botName}")]
    [ServiceFilter<IpWhitelistFilter>]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [SwaggerOperation(Description = $"Protected by *IP whitelisting*.\r\n\r\n")]
    public async Task<IActionResult> GetBotActivityByName(string botName)
    {
        return this.Ok(await this.botStatusService.GetActivitiesForBot(botName, this.HttpContext.RequestAborted));
    }

    [HttpGet("bot-activity/maps/{map}")]
    [ServiceFilter<IpWhitelistFilter>]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [SwaggerOperation(Description = $"Protected by *IP whitelisting*.\r\n\r\n")]
    public async Task<IActionResult> GetBotActivityByMap(string map)
    {
        if (int.TryParse(map, out var mapId))
        {
            return this.Ok(await this.botStatusService.GetActivitiesForMap(mapId, this.HttpContext.RequestAborted));
        }
        else
        {
            return this.Ok(await this.botStatusService.GetActivitiesForMap(map, this.HttpContext.RequestAborted));
        }
    }

    [HttpGet("bots")]
    [ServiceFilter<IpWhitelistFilter>]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [SwaggerOperation(Description = $"Protected by *IP whitelisting*.\r\n\r\n")]
    public async Task<IActionResult> GetBotStatus()
    {
        return this.Ok(await this.botStatusService.GetBots(this.HttpContext.RequestAborted));
    }

    [HttpGet("map-activity")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetActiveMaps()
    {
        var bots = await this.botStatusService.GetBots(this.HttpContext.RequestAborted);
        return this.Ok(bots.Select(b => new MapActivity
        {
            District = b.District,
            MapId = b.Map?.Id ?? -1,
            LastUpdate = b.LastSeen ?? DateTime.MinValue
        }));
    }
}
