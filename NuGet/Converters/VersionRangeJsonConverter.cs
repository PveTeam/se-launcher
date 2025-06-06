using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace NuGet.Converters;

public class VersionRangeJsonConverter : JsonConverter<VersionRange>
{
    public override VersionRange? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Invalid version range");

        return VersionRange.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, VersionRange value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}