namespace GuildWarsPartySearch.Server.Models.Endpoints;

public abstract class GetPartySearchFailure
{
    public string? Message { get; init; }

    public sealed class InvalidPayload : GetPartySearchFailure
    {
    }

    public sealed class InvalidCampaign : GetPartySearchFailure
    {
    }

    public sealed class InvalidContinent : GetPartySearchFailure
    {
    }

    public sealed class InvalidRegion : GetPartySearchFailure
    {
    }

    public sealed class InvalidMap : GetPartySearchFailure
    {
    }

    public sealed class InvalidDistrict : GetPartySearchFailure
    {
    }

    public sealed class EntriesNotFound : GetPartySearchFailure
    {
    }

    public sealed class UnspecifiedFailure : GetPartySearchFailure
    {
    }
}
