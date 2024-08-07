using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace GuildWarsPartySearch.Server.Endpoints;

[Route("models")]
public class ModelsController : Controller
{
    [HttpGet("maps")]
    [ProducesResponseType(200)]
    public IActionResult GetMaps()
    {
        return this.Ok(Map.Maps.Select(m => new MapResponse
        {
            Id = m.Id,
            Name = m.Name
        }));
    }

    [HttpGet("professions")]
    [ProducesResponseType(200)]
    public IActionResult GetProfessions()
    {
        return this.Ok(Profession.Professions.Select(p => new ProfessionResponse
        {
            Name = p.Name,
            Alias = p.Alias,
            Id = p.Id
        }));
    }
}
