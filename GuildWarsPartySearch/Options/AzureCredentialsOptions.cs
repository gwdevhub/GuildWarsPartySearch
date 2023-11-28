using GuildWarsPartySearch.Server.Options.Azure;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Options;

public sealed class AzureCredentialsOptions : IAzureClientSecretCredentialOptions
{
    [JsonPropertyName(nameof(ClientSecret))]
    public string ClientSecret { get; set; } = default!;

    [JsonPropertyName(nameof(ClientId))]
    public string ClientId { get; set; } = default!;

    [JsonPropertyName(nameof(TenantId))]
    public string TenantId { get; set; } = default!;
}
