using GuildWarsPartySearch.Server.Models;

namespace GuildWarsPartySearch.Server.Services.Database.Models;

public sealed class ApiKeyModel
{
    public string? Key { get; set; }
    public string? Description { get; set; }
    public PermissionLevel? PermissionLevel { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? LastUsedTime { get; set; }
}
