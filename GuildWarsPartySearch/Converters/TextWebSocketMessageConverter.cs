using GuildWarsPartySearch.Server.Models;
using GuildWarsPartySearch.Server.Models.Endpoints;
using System.Text;

namespace GuildWarsPartySearch.Server.Converters;

public sealed class TextWebSocketMessageConverter : WebSocketMessageConverter<TextContent>
{
    public override TextContent ConvertTo(WebSocketConverterRequest request)
    {
        if (request.Type is not System.Net.WebSockets.WebSocketMessageType.Text)
        {
            throw new InvalidOperationException($"Cannot parse message of type {request.Type}");
        }

        var message = Encoding.UTF8.GetString(request.Payload!);
        return new TextContent { Text = message };
    }

    public override WebSocketConverterResponse ConvertFrom(TextContent message)
    {
        return new WebSocketConverterResponse
        {
            Type = System.Net.WebSockets.WebSocketMessageType.Text,
            EndOfMessage = true,
            Payload = Encoding.UTF8.GetBytes(message.Text ?? string.Empty)
        };
    }
}
