namespace GuildWarsPartySearch.Server.Models.Endpoints;

public abstract class PostPartySearchFailure
{
    public string? Message { get; init; }

    public sealed class InvalidPayload : PostPartySearchFailure
    {
    }

    public sealed class InvalidCampaign : PostPartySearchFailure
    {
    }

    public sealed class InvalidContinent : PostPartySearchFailure
    {
    }

    public sealed class InvalidRegion : PostPartySearchFailure
    {
    }

    public sealed class InvalidMap : PostPartySearchFailure
    {
    }

    public sealed class InvalidDistrict : PostPartySearchFailure
    {
    }

    public sealed class InvalidEntries : PostPartySearchFailure
    {
    }

    public sealed class InvalidPartySize : PostPartySearchFailure
    {
    }

    public sealed class InvalidPartyMaxSize : PostPartySearchFailure
    {
    }

    public sealed class InvalidNpcs : PostPartySearchFailure
    {
    }

    public sealed class InvalidCharName : PostPartySearchFailure
    {
    }

    public sealed class UnspecifiedFailure : PostPartySearchFailure
    {
    }
}
