using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Models;

namespace NuGet.Converters;

public class RuntimeFrameworkJsonConverter : JsonConverter<NuGetRuntimeFramework>
{
    public override NuGetRuntimeFramework Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is not (JsonTokenType.String or JsonTokenType.PropertyName))
            throw new JsonException("Invalid runtime framework string");
        
        return NuGetRuntimeFramework.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, NuGetRuntimeFramework value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }

    public override NuGetRuntimeFramework ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options) => Read(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, NuGetRuntimeFramework value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString());
    }
}