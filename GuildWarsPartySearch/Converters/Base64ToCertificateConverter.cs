using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace GuildWarsPartySearch.Server.Converters;

public sealed class Base64ToCertificateConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(X509Certificate2);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        if (reader.Value is not string base64)
        {
            throw new InvalidOperationException($"Cannot convert {reader.Value} to {nameof(X509Certificate2)}");
        }

        var bytes = Convert.FromBase64String(base64);
        return new X509Certificate2(bytes);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        if (value is not X509Certificate2 certificate2)
        {
            throw new InvalidOperationException($"Cannot convert {value} as {nameof(X509Certificate2)}");
        }

        var base64 = Convert.ToBase64String(certificate2.GetRawCertData());
        writer.WriteValue(base64);
    }
}
