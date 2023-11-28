namespace GuildWarsPartySearch.Server.Options.Azure;

public interface IAzureClientSecretCredentialOptions : IAzureCredentialOptions
{
    string ClientSecret { get; set; }
}
