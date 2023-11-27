using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GuildWarsPartySearch.Server.Converters;

public sealed class Base64ToCertificateConverter : JsonConverter<X509Certificate2>
{
    public override X509Certificate2? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.GetString() is not string base64)
        {
            throw new InvalidOperationException($"Cannot deserialize {nameof(X509Certificate2)} from {reader.GetString()}");
        }

        var bytes = Convert.FromBase64String(base64);
        return new X509Certificate2(bytes);
    }

    public override void Write(Utf8JsonWriter writer, X509Certificate2 value, JsonSerializerOptions options)
    {
        var base64 = Convert.ToBase64String(value!.GetRawCertData());
        writer.WriteStringValue(base64);
    }
}
