using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

public class MapResponse
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }
}
