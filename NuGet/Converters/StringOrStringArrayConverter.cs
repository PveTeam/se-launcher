using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NuGet.Converters;

public class StringOrStringArrayConverter : JsonConverter<ImmutableArray<string>>
{
    public override ImmutableArray<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return [reader.GetString()!];
            case JsonTokenType.StartArray:
            {
                var builder = ImmutableArray.CreateBuilder<string>();

                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    builder.Add(reader.GetString()!);
                }

                return builder.ToImmutable();
            }
            default:
                throw new JsonException("String or array of strings expected");
        }
    }

    public override void Write(Utf8JsonWriter writer, ImmutableArray<string> value, JsonSerializerOptions options)
    {
        if (value.Length == 1)
        {
            writer.WriteStringValue(value[0]);
            return;
        }

        writer.WriteStartArray();

        foreach (var author in value)
        {
            writer.WriteStringValue(author);
        }

        writer.WriteEndArray();
    }
}