using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace GuildWarsPartySearch.Server.Converters;

public sealed class Base64ToCertificateConverter : JsonConverter<X509Certificate2>
{
    public override X509Certificate2? ReadJson(JsonReader reader, Type objectType, X509Certificate2? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.Value is not string base64)
        {
            throw new InvalidOperationException($"Cannot deserialize {nameof(X509Certificate2)} from {reader.Value}");
        }

        var bytes = Convert.FromBase64String(base64);
        return new X509Certificate2(bytes);
    }

    public override void WriteJson(JsonWriter writer, X509Certificate2? value, JsonSerializer serializer)
    {
        var base64 = Convert.ToBase64String(value!.GetRawCertData());
        writer.WriteValue(base64);
    }
}
