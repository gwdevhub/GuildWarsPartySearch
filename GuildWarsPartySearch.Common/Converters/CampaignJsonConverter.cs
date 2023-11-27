using GuildWarsPartySearch.Common.Models.GuildWars;
using Newtonsoft.Json;

namespace GuildWarsPartySearch.Common.Converters;
public sealed class CampaignJsonConverter : JsonConverter
{
    public override bool CanConvert(Type objectType) => true;

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (!objectType.IsAssignableTo(typeof(Campaign)))
        {
            return default;
        }

        switch (reader.TokenType)
        {
            case JsonToken.String:
                var name = reader.Value as string;
                if (name is null ||
                    !Campaign.TryParse(name, out var namedCampaign))
                {
                    return default;
                }

                return namedCampaign;
            case JsonToken.Integer:
                var id = reader.Value as long?;
                if (id is not long ||
                    !Campaign.TryParse((int)id.Value, out var parsedCampaign))
                {
                    return default;
                }

                return parsedCampaign;

            default:
                return default;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not Campaign campaign)
        {
            return;
        }

        writer.WriteValue(campaign.Name);
    }
}
