using GuildWarsPartySearch.Common.Models.GuildWars;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Common.Converters;
public sealed class CampaignJsonConverter : JsonConverter<Campaign>
{
    public override Campaign? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var name = reader.GetString();
                if (name is null ||
                    !Campaign.TryParse(name, out var namedCampaign))
                {
                    return default;
                }

                return namedCampaign;
            case JsonTokenType.Number:
                reader.TryGetInt64(out var id);
                if (!Campaign.TryParse((int)id, out var parsedCampaign))
                {
                    return default;
                }

                return parsedCampaign;

            default:
                return default;
        }
    }

    public override void Write(Utf8JsonWriter writer, Campaign value, JsonSerializerOptions options)
    {
        if (value is not Campaign campaign)
        {
            return;
        }

        writer.WriteNumberValue(campaign.Id);
    }
}
