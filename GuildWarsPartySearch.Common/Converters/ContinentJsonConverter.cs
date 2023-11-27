using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Common.Converters;
public sealed class ContinentJsonConverter : JsonConverter<Continent>
{
    public override Continent? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var name = reader.GetString();
                if (name is null ||
                    !Continent.TryParse(name, out var namedContinent))
                {
                    return default;
                }

                return namedContinent;
            case JsonTokenType.Number:
                reader.TryGetInt64(out var id);
                if (!Continent.TryParse((int)id, out var parsedContinent))
                {
                    return default;
                }

                return parsedContinent;

            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, Continent value, JsonSerializerOptions options)
    {
        if (value is not Continent continent)
        {
            return;
        }

        writer.WriteStringValue(continent.Name);
    }
}
