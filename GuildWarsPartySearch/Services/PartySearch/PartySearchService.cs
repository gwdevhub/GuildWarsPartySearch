using GuildWarsPartySearch.Common.Models.GuildWars;
using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Models.Endpoints;
using GuildWarsPartySearch.Server.Services.CharName;
using GuildWarsPartySearch.Server.Services.Database;
using System.Core.Extensions;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Services.PartySearch;

public sealed class PartySearchService : IPartySearchService
{
    private readonly ICharNameValidator charNameValidator;
    private readonly IPartySearchDatabase partySearchDatabase;
    private readonly ILogger<PartySearchService> logger;

    public PartySearchService(
        ICharNameValidator charNameValidator,
        IPartySearchDatabase partySearchDatabase,
        ILogger<PartySearchService> logger)
    {
        this.charNameValidator = charNameValidator.ThrowIfNull();
        this.partySearchDatabase = partySearchDatabase.ThrowIfNull();
        this.logger = logger.ThrowIfNull();
    }

    public async Task<Result<List<Models.PartySearch>, GetPartySearchFailure>> GetPartySearchesByCharName(string charName, CancellationToken cancellationToken)
    {
        if (!this.charNameValidator.Validate(charName))
        {
            return new GetPartySearchFailure.InvalidCharName();
        }

        return await this.partySearchDatabase.GetPartySearchesByCharName(charName, cancellationToken);
    }

    public async Task<Result<List<Models.PartySearch>, GetPartySearchFailure>> GetPartySearchesByCampaign(string campaign, CancellationToken cancellationToken)
    {
        if (int.TryParse(campaign, out var id) &&
            Campaign.TryParse(id, out var parsedCampaign))
        {
            return await this.partySearchDatabase.GetPartySearchesByCampaign(parsedCampaign, cancellationToken);
        }
        
        if (Campaign.TryParse(campaign, out var namedCampaign))
        {
            return await this.partySearchDatabase.GetPartySearchesByCampaign(namedCampaign, cancellationToken);
        }

        return new GetPartySearchFailure.InvalidCampaign();
    }

    public async Task<Result<List<Models.PartySearch>, GetPartySearchFailure>> GetPartySearchesByContinent(string continent, CancellationToken cancellationToken)
    {
        if (int.TryParse(continent, out var id) &&
            Continent.TryParse(id, out var parsedContinent))
        {
            return await this.partySearchDatabase.GetPartySearchesByContinent(parsedContinent, cancellationToken);
        }

        if (Continent.TryParse(continent, out var namedContinent))
        {
            return await this.partySearchDatabase.GetPartySearchesByContinent(namedContinent, cancellationToken);
        }

        return new GetPartySearchFailure.InvalidContinent();
    }

    public async Task<Result<List<Models.PartySearch>, GetPartySearchFailure>> GetPartySearchesByRegion(string region, CancellationToken cancellationToken)
    {
        if (int.TryParse(region, out var id) &&
            Region.TryParse(id, out var parsedRegion))
        {
            return await this.partySearchDatabase.GetPartySearchesByRegion(parsedRegion, cancellationToken);
        }

        if (Region.TryParse(region, out var namedRegion))
        {
            return await this.partySearchDatabase.GetPartySearchesByRegion(namedRegion, cancellationToken);
        }

        return new GetPartySearchFailure.InvalidRegion();
    }

    public async Task<Result<List<Models.PartySearch>, GetPartySearchFailure>> GetPartySearchesByMap(string map, CancellationToken cancellationToken)
    {
        if (int.TryParse(map, out var id) &&
            Map.TryParse(id, out var parsedMap))
        {
            return await this.partySearchDatabase.GetPartySearchesByMap(parsedMap, cancellationToken);
        }

        if (Map.TryParse(map, out var namedMap))
        {
            return await this.partySearchDatabase.GetPartySearchesByMap(namedMap, cancellationToken);
        }

        return new GetPartySearchFailure.InvalidMap();
    }

    public Task<List<Models.PartySearch>> GetAllPartySearches(CancellationToken cancellationToken)
    {
        return this.partySearchDatabase.GetAllPartySearches(cancellationToken);
    }

    public async Task<Result<List<PartySearchEntry>, GetPartySearchFailure>> GetPartySearch(Campaign? campaign, Continent? continent, Region? region, Map? map, string? district, CancellationToken cancellationToken)
    {
        if (campaign is null)
        {
            return new GetPartySearchFailure.InvalidCampaign();
        }

        if (continent is null)
        {
            return new GetPartySearchFailure.InvalidContinent();
        }

        if (region is null)
        {
            return new GetPartySearchFailure.InvalidRegion();
        }

        if (map is null)
        {
            return new GetPartySearchFailure.InvalidMap();
        }

        if (district?.IsNullOrWhiteSpace() is not false)
        {
            return new GetPartySearchFailure.InvalidDistrict();
        }

        //TODO: Validate district

        var result = await this.partySearchDatabase.GetPartySearches(campaign, continent, region, map, district, cancellationToken);
        if (result is not List<PartySearchEntry> entries)
        {
            return new GetPartySearchFailure.EntriesNotFound();
        }

        return entries.ToList();
    }

    public async Task<Result<PostPartySearchRequest, PostPartySearchFailure>> PostPartySearch(PostPartySearchRequest? request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return new PostPartySearchFailure.InvalidPayload();
        }

        if (request.Campaign is null)
        {
            return new PostPartySearchFailure.InvalidCampaign();
        }

        if (request.Continent is null)
        {
            return new PostPartySearchFailure.InvalidContinent();
        }

        if (request.Region is null)
        {
            return new PostPartySearchFailure.InvalidRegion();
        }

        if (request.Map is null)
        {
            return new PostPartySearchFailure.InvalidMap();
        }

        if (request.District is null)
        {
            return new PostPartySearchFailure.InvalidDistrict();
        }

        if (request.PartySearchEntries is null)
        {
            return new PostPartySearchFailure.InvalidEntries();
        }

        foreach(var entry in request.PartySearchEntries)
        {
            if (entry.PartySize is null)
            {
                return new PostPartySearchFailure.InvalidPartySize();
            }

            if (entry.PartyMaxSize is null)
            {
                return new PostPartySearchFailure.InvalidPartyMaxSize();
            }

            if (entry.Npcs is null)
            {
                return new PostPartySearchFailure.InvalidNpcs();
            }

            if (entry.CharName is null)
            {
                return new PostPartySearchFailure.InvalidCharName();
            }
        }

        //TODO: Implement district validation, party size validation, party max size validation and npcs validation
        var result = await this.partySearchDatabase.SetPartySearches(
            request.Campaign,
            request.Continent,
            request.Region,
            request.Map,
            request.District,
            request.PartySearchEntries,
            cancellationToken);

        if (!result)
        {
            return new PostPartySearchFailure.UnspecifiedFailure
            {
                Message = "Database operation unsuccessful"
            };
        }

        return request;
    }
}
