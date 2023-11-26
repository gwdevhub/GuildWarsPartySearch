using GuildWarsPartySearch.Server.Models;
using System.Text;
using Newtonsoft.Json;

namespace GuildWarsPartySearch.Server.Converters;

public class JsonWebSocketMessageConverter<T> : WebSocketMessageConverter<T>
{
    public override T ConvertTo(WebSocketConverterRequest request)
    {
        if (request.Type != System.Net.WebSockets.WebSocketMessageType.Text)
        {
            throw new InvalidOperationException($"Unable to deserialize message. Message is not text");
        }

        var stringData = Encoding.UTF8.GetString(request.Payload!);
        var objData = JsonConvert.DeserializeObject<T>(stringData);
        return objData ?? throw new InvalidOperationException($"Unable to deserialize message to {typeof(T).Name}");
    }

    public override WebSocketConverterResponse ConvertFrom(T message)
    {
        var serialized = JsonConvert.SerializeObject(message);
        var data = Encoding.UTF8.GetBytes(serialized);
        return new WebSocketConverterResponse { EndOfMessage = true, Type = System.Net.WebSockets.WebSocketMessageType.Text, Payload = data };
    }
}
