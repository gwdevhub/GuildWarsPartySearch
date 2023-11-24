using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

public sealed class PartySearchEntry
{
    [JsonProperty(nameof(PartySize))]
    public int? PartySize { get; set; }

    [JsonProperty(nameof(PartyMaxSize))]
    public int? PartyMaxSize { get; set; }

    [JsonProperty(nameof(Npcs))]
    public int? Npcs { get; set; }
}
