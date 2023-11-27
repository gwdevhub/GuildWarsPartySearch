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
            onSuccess: list => this.Ok(list),
            onFailure: failure => failure switch
            {
                GetPartySearchFailure.InvalidCharName => this.BadRequest("Invalid char name"),
                _ => this.Problem()
            });
    }

    [HttpGet("campaigns/{campaign}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetByCampaign(string campaign)
    {
        var result = await this.partySearchService.GetPartySearchesByCampaign(campaign, this.HttpContext.RequestAborted);
        return result.Switch<IActionResult>(
            onSuccess: list => this.Ok(list),
            onFailure: failure => failure switch
            {
                GetPartySearchFailure.InvalidCampaign => this.BadRequest("Invalid campaign"),
                _ => this.Problem()
            });
    }

    [HttpGet("continents/{continent}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetByContinent(string continent)
    {
        var result = await this.partySearchService.GetPartySearchesByContinent(continent, this.HttpContext.RequestAborted);
        return result.Switch<IActionResult>(
            onSuccess: list => this.Ok(list),
            onFailure: failure => failure switch
            {
                GetPartySearchFailure.InvalidContinent => this.BadRequest("Invalid continent"),
                _ => this.Problem()
            });
    }

    [HttpGet("regions/{region}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> GetByRegion(string region)
    {
        var result = await this.partySearchService.GetPartySearchesByRegion(region, this.HttpContext.RequestAborted);
        return result.Switch<IActionResult>(
            onSuccess: list => this.Ok(list),
            onFailure: failure => failure switch
            {
                GetPartySearchFailure.InvalidRegion => this.BadRequest("Invalid region"),
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
            onSuccess: list => this.Ok(list),
            onFailure: failure => failure switch
            {
                GetPartySearchFailure.InvalidMap => this.BadRequest("Invalid map"),
                _ => this.Problem()
            });
    }
}
