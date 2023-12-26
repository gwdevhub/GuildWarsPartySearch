using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.AspNetCore.Mvc;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[Route("party-search")]
public sealed class PartySearchController : Controller
{
    private readonly IPartySearchService partySearchService;

    public PartySearchController(
        IPartySearchService partySearchService)
    {
        this.partySearchService = partySearchService.ThrowIfNull();
    }

    [HttpGet("characters/{charName}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetByCharName(string charName)
    {
        var result = await this.partySearchService.GetPartySearchesByCharName(charName, this.HttpContext.RequestAborted);
        return result.Switch<IActionResult>(
            onSuccess: this.Ok,
            onFailure: failure => failure switch
            {
                GetPartySearchFailure.InvalidCharName => this.BadRequest("Invalid char name"),
                _ => this.Problem()
            });
    }

    [HttpGet("maps/{map}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetByMap(string map)
    {
        var result = await this.partySearchService.GetPartySearchesByMap(map, this.HttpContext.RequestAborted);
        return result.Switch<IActionResult>(
            onSuccess: this.Ok,
            onFailure: failure => failure switch
            {
                GetPartySearchFailure.InvalidMap => this.BadRequest("Invalid map"),
                _ => this.Problem()
            });
    }
}
