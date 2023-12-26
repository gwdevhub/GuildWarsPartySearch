using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace GuildWarsPartySearch.Common.Converters;
public sealed class ProfessionJsonConverter : JsonConverter<Profession>
{
    public override Profession? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var name = reader.GetString();
                if (name is null ||
                    !Profession.TryParse(name, out var namedProfession))
                {
                    return default;
                }

                return namedProfession;
            case JsonTokenType.Number:
                reader.TryGetInt64(out var id);
                if (!Profession.TryParse((int)id, out var parsedProfession))
                {
                    return default;
                }

                return parsedProfession;

            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, Profession value, JsonSerializerOptions options)
    {
        if (value is not Profession profession)
        {
            return;
        }

        writer.WriteNumberValue(profession.Id);
    }
}
