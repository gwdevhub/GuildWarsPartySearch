namespace GuildWarsPartySearch.Server.Services.Database.Models;

public class BotActivity
{
    public enum ActivityType
    {
        None,
        Connect,
        Disconnect,
        Update
    }

    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int MapId { get; set; }
    public ActivityType Activity { get; set; }
    public DateTime TimeStamp { get; set; }
}
