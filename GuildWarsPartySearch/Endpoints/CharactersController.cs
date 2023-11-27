using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.PartySearch;
using Microsoft.AspNetCore.Mvc;
using System.Core.Extensions;

namespace GuildWarsPartySearch.Server.Endpoints;

[Route("party-search/characters")]
public sealed class CharactersController : Controller
{
    private readonly IPartySearchService partySearchService;

    public CharactersController(
        IPartySearchService partySearchService)
    {
        this.partySearchService = partySearchService.ThrowIfNull();
    }

    [HttpGet("{charName}")]
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
}
