namespace GuildWarsPartySearch.Server.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public sealed class OptionsNameAttribute : Attribute
{
    public string? Name { get; set; }
}
