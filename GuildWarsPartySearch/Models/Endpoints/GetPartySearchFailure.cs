namespace GuildWarsPartySearch.Server.Models.Endpoints;

public abstract class GetPartySearchFailure
{
    public string? Message { get; init; }

    public sealed class InvalidPayload : GetPartySearchFailure
    {
    }

    public sealed class InvalidDistrictRegion : GetPartySearchFailure
    {
    }

    public sealed class InvalidDistrictNumber : GetPartySearchFailure
    {
    }

    public sealed class InvalidDistrictLanguage : GetPartySearchFailure
    {
    }

    public sealed class InvalidMap : GetPartySearchFailure
    {
    }

    public sealed class InvalidCharName : GetPartySearchFailure
    {
    }

    public sealed class EntriesNotFound : GetPartySearchFailure
    {
    }

    public sealed class UnspecifiedFailure : GetPartySearchFailure
    {
    }
}
