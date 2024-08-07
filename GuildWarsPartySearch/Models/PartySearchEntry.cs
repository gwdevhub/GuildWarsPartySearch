using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Models;

public sealed class PartySearchEntry
{
    [JsonPropertyName("party_id")]
    public int PartyId { get; set; }

    [JsonPropertyName("district")]
    public int District { get; set; }

    [JsonPropertyName("district_number")]
    public int DistrictNumber { get; set; }

    [JsonPropertyName("language")]
    public DistrictLanguage? DistrictLanguage { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("sender")]
    public string? Sender { get; set; }

    [JsonPropertyName("party_size")]
    public int PartySize { get; set; }

    [JsonPropertyName("hero_count")]
    public int HeroCount { get; set; }

    [JsonPropertyName("hardmode")]
    public HardModeState? HardMode { get; set; }

    [JsonPropertyName("search_type")]
    public int SearchType { get; set; }

    [JsonPropertyName("primary")]
    public Profession? Primary { get; set; }

    [JsonPropertyName("secondary")]
    public Profession? Secondary { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; }
}
