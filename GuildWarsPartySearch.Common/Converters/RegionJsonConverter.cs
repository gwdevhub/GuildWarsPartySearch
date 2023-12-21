using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Common.Converters;
public sealed class RegionJsonConverter : JsonConverter<Region>
{
    public override Region? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var name = reader.GetString();
                if (name is null ||
                    !Region.TryParse(name, out var namedRegion))
                {
                    return default;
                }

                return namedRegion;
            case JsonTokenType.Number:
                reader.TryGetInt64(out var id);
                if (!Region.TryParse((int)id, out var parsedRegion))
                {
                    return default;
                }

                return parsedRegion;

            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, Region value, JsonSerializerOptions options)
    {
        if (value is not Region region)
        {
            return;
        }

        writer.WriteNumberValue(region.Id);
    }
}
