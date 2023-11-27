using GuildWarsPartySearch.Server.Attributes;
using GuildWarsPartySearch.Server.Models;
using System.Core.Extensions;
using System.Text;
using System.Text.Json;

namespace GuildWarsPartySearch.Server.Converters;

public class JsonWebSocketMessageConverter<T> : WebSocketMessageConverter<T>
{
    private readonly JsonSerializerOptions jsonSerializerOptions;

    [DoNotInject]
    public JsonWebSocketMessageConverter()
    {
    }

    public JsonWebSocketMessageConverter(JsonSerializerOptions options)
    {
        this.jsonSerializerOptions = options.ThrowIfNull();
    }

    public override T ConvertTo(WebSocketConverterRequest request)
    {
        if (request.Type != System.Net.WebSockets.WebSocketMessageType.Text)
        {
            throw new InvalidOperationException($"Unable to deserialize message. Message is not text");
        }

        var stringData = Encoding.UTF8.GetString(request.Payload!);
        var objData = JsonSerializer.Deserialize<T>(stringData, this.jsonSerializerOptions);
        return objData ?? throw new InvalidOperationException($"Unable to deserialize message to {typeof(T).Name}");
    }

    public override WebSocketConverterResponse ConvertFrom(T message)
    {
        var serialized = JsonSerializer.Serialize(message, this.jsonSerializerOptions);
        var data = Encoding.UTF8.GetBytes(serialized);
        return new WebSocketConverterResponse { EndOfMessage = true, Type = System.Net.WebSockets.WebSocketMessageType.Text, Payload = data };
    }
}
