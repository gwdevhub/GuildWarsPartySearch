using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Models;

public sealed class WebSocketConverterRequest
{
    public WebSocketMessageType Type { get; set; }
    public byte[]? Payload { get; set; }
}
