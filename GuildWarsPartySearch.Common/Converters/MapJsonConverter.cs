using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Common.Converters;
public sealed class MapJsonConverter : JsonConverter<Map>
{
    public override Map? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var name = reader.GetString();
                if (name is null ||
                    !Map.TryParse(name, out var namedMap))
                {
                    return default;
                }

                return namedMap;
            case JsonTokenType.Number:
                reader.TryGetInt64(out var id);
                if (!Map.TryParse((int)id, out var parsedMap))
                {
                    return default;
                }

                return parsedMap;

            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, Map value, JsonSerializerOptions options)
    {
        if (value is not Map map)
        {
            return;
        }

        writer.WriteNumberValue(map.Id);
    }
}
