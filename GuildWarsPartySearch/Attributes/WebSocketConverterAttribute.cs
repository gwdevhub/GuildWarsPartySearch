using GuildWarsPartySearch.Server.Converters;

namespace GuildWarsPartySearch.Server.Attributes;

public abstract class WebSocketConverterAttributeBase : Attribute
{
    public abstract Type ConverterType { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class WebSocketConverterAttribute<TConverter, TPayload> : WebSocketConverterAttributeBase
    where TConverter : WebSocketMessageConverter<TPayload>, new()
{
    public override sealed Type ConverterType => typeof(TConverter);
}
