using GuildWarsPartySearch.Server.Models;

namespace GuildWarsPartySearch.Server.Filters;

public class AdminPermissionRequired : PermissionFilterBase
{
    public override PermissionLevel PermissionLevel { get; } = PermissionLevel.Admin;
}
