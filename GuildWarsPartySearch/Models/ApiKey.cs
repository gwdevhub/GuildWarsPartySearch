namespace GuildWarsPartySearch.Server.Models;

public sealed class ApiKey
{
    public string Key { get; init; } = default!;
    public bool Deletable { get; init; } = default!;
    public PermissionLevel PermissionLevel { get; init; }
    public string Description { get; init; } = default!;
    public DateTime CreationTime { get; init; }
    public DateTime LastUsedTime { get; init; }
}
