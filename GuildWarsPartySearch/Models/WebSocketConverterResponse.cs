using System.Net.WebSockets;

namespace GuildWarsPartySearch.Server.Models;

public sealed class WebSocketConverterResponse
{
    public WebSocketMessageType Type { get; set; }
    public byte[]? Payload { get; set; }
    public bool EndOfMessage {  get; set; }
}
