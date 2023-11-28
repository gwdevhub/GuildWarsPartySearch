namespace GuildWarsPartySearch.Server.Options.Azure;

public interface IAzureCredentialOptions
{
    string ClientId { get; set; }

    string TenantId { get; set; }
}
