using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Models;

namespace NuGet.Converters;

public class ResourceTypeJsonConverter : JsonConverter<ResourceType>
{
    public override ResourceType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Invalid resource type");

        return ResourceType.Parse(reader.GetString()!);
    }

    public override void Write(Utf8JsonWriter writer, ResourceType value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}