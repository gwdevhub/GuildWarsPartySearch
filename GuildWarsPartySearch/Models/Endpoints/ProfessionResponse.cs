using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

public class ProfessionResponse
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
}
