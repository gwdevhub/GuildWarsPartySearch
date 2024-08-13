using GuildWarsPartySearch.Server.Models;

namespace GuildWarsPartySearch.Server.Options;

public class ApiWhitelistOptions
{
    public string TableName { get; set; } = "api_keys";
    public List<ApiKey> Keys { get; set; } = [new(){ Key = "development", PermissionLevel = PermissionLevel.Admin, CreationTime = DateTime.Now, LastUsedTime = DateTime.Now, Description = "Default admin"}];
}
