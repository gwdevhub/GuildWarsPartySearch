using GuildWarsPartySearch.Server.Filters;
using GuildWarsPartySearch.Server.Services.BotStatus;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[Route("status")]
[ServiceFilter<ApiKeyProtected>]
[ServiceFilter<RequireSsl>]
public class StatusController : Controller
{
    private readonly IBotStatusService botStatusService;

    public StatusController(
        IBotStatusService botStatusService)
    {
        this.botStatusService = botStatusService.ThrowIfNull();
    }

    [HttpGet("bots")]
    [ProducesResponseType(200)]
    [ProducesResponseType(403)]
    [SwaggerOperation(Description = $"Protected by *{ApiKeyProtected.ApiKeyHeader}* header.\r\n\r\nRequires *SSL* protocol. (https://)")]
    public async Task<IActionResult> GetBotStatus([FromHeader(Name = ApiKeyProtected.ApiKeyHeader)] string _)
    {
        return this.Ok(await this.botStatusService.GetBots());
    }
}
