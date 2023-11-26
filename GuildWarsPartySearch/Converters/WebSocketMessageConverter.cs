using GuildWarsPartySearch.Server.Models;
using System.Extensions;

namespace GuildWarsPartySearch.Server.Converters;

public abstract class WebSocketMessageConverterBase
{
    public abstract object ConvertToObject(WebSocketConverterRequest request);
    public abstract WebSocketConverterResponse ConvertFromObject(object message);
}

public abstract class WebSocketMessageConverter<T> : WebSocketMessageConverterBase
{
    public sealed override object ConvertToObject(WebSocketConverterRequest request)
    {
        return this.ConvertTo(request)!;
    }
    public sealed override WebSocketConverterResponse ConvertFromObject(object message)
    {
        return this.ConvertFrom(message.Cast<T>());
    }

    public abstract T ConvertTo(WebSocketConverterRequest request);
    public abstract WebSocketConverterResponse ConvertFrom(T message);
}
