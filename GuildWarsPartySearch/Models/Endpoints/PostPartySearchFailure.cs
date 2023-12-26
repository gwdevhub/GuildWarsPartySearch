namespace GuildWarsPartySearch.Server.Models.Endpoints;

public abstract class PostPartySearchFailure
{
    public string? Message { get; init; }

    public sealed class InvalidPayload : PostPartySearchFailure
    {
    }

    public sealed class InvalidDistrictRegion : PostPartySearchFailure
    {
    }

    public sealed class InvalidDistrictLanguage : PostPartySearchFailure
    {
    }

    public sealed class InvalidDistrictNumber : PostPartySearchFailure
    {
    }

    public sealed class InvalidPartyId : PostPartySearchFailure
    {
    }

    public sealed class InvalidMessage : PostPartySearchFailure
    {
    }

    public sealed class InvalidEntries : PostPartySearchFailure
    {
    }

    public sealed class InvalidSender : PostPartySearchFailure
    {
    }

    public sealed class InvalidHeroCount : PostPartySearchFailure
    {
    }

    public sealed class InvalidHardMode : PostPartySearchFailure
    {
    }

    public sealed class InvalidSearchType : PostPartySearchFailure
    {
    }

    public sealed class InvalidPrimary : PostPartySearchFailure
    {
    }

    public sealed class InvalidSecondary : PostPartySearchFailure
    {
    }

    public sealed class InvalidLevel : PostPartySearchFailure
    {
    }

    public sealed class InvalidMap : PostPartySearchFailure
    {
    }

    public sealed class UnspecifiedFailure : PostPartySearchFailure
    {
    }
}
