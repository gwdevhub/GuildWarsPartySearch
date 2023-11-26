using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Converters;

namespace GuildWarsPartySearch.Server.Models.Endpoints;

[WebSocketConverter<TextWebSocketMessageConverter, TextContent>]
public sealed class TextContent
{
    public string? Text { get; set; }
}
