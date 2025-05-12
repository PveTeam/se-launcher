using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Deps;

namespace NuGet.Converters;

public class ManifestPackageKeyJsonConverter : JsonConverter<ManifestPackageKey>
{
    public override ManifestPackageKey Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.PropertyName))
            throw new JsonException("Invalid package key string");
        
        return ManifestPackageKey.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, ManifestPackageKey value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override ManifestPackageKey ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options) => Read(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, ManifestPackageKey value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}