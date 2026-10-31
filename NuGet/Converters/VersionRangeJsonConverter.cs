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

        var str = reader.GetString();
        return string.IsNullOrEmpty(str) ? VersionRange.AllStable : VersionRange.Parse(str);
    }

    public override void Write(Utf8JsonWriter writer, VersionRange value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
