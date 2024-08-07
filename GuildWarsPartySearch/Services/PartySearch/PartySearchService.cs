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

    public async Task<Result<List<PartySearchEntry>, GetPartySearchFailure>> GetPartySearch(Map? map, int? district, CancellationToken cancellationToken)
    {
        if (map is null)
        {
            return new GetPartySearchFailure.InvalidMap();
        }

        if (district is null)
        {
            return new GetPartySearchFailure.InvalidDistrictRegion();
        }

        var result = await this.partySearchDatabase.GetPartySearches(map, district.Cast<int>(), cancellationToken);
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

        if (request.Map is null)
        {
            return new PostPartySearchFailure.InvalidMap();
        }

        if (request.District is null)
        {
            return new PostPartySearchFailure.InvalidDistrictRegion();
        }

        if (request.PartySearchEntries is null)
        {
            return new PostPartySearchFailure.InvalidEntries();
        }

        foreach(var entry in request.PartySearchEntries)
        {
            if (entry.DistrictLanguage is not DistrictLanguage)
            {
                return new PostPartySearchFailure.InvalidDistrictLanguage();
            }

            if (entry.HardMode is not HardModeState)
            {
                return new PostPartySearchFailure.InvalidHardMode();
            }

            if (entry.Sender?.IsNullOrWhiteSpace() is not false)
            {
                return new PostPartySearchFailure.InvalidSender();
            }

            if (entry.Primary is null)
            {
                return new PostPartySearchFailure.InvalidPrimary();
            }

            if (entry.Secondary is null)
            {
                return new PostPartySearchFailure.InvalidSecondary();
            }
        }

        //TODO: Implement district validation, party size validation, party max size validation and npcs validation
        var result = await this.partySearchDatabase.SetPartySearches(
            request.Map,
            request.District.Cast<int>(),
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
