namespace GuildWarsPartySearch.Server.Services.Database.Models;

public sealed class PartySearchTableEntity
{
    public int DistrictNumber { get; set; }

    public int DistrictLanguage { get; set; }

    public int District { get; set; }

    public int MapId { get; set; }

    public int PartyId { get; set; }

    public string? Message { get; set; }

    public string? Sender { get; set; }

    public int PartySize { get; set; }

    public int HeroCount { get; set; }

    public int HardMode { get; set; }

    public int SearchType { get; set; }

    public int Primary { get; set; }

    public int Secondary { get; set; }

    public int Level { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
