using GuildWarsPartySearch.Server.Models;

namespace GuildWarsPartySearch.Server.Filters;

public sealed class BotPermissionRequired : PermissionFilterBase
{
    public override PermissionLevel PermissionLevel { get; } = PermissionLevel.Bot;
}
