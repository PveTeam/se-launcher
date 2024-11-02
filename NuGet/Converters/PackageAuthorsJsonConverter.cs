using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Models;

namespace NuGet.Converters;

public class PackageAuthorsJsonConverter : JsonConverter<PackageAuthors>
{
    public override PackageAuthors? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
            {
                var author = reader.GetString()!;
                return new PackageAuthors(author, [author]);
            }
            case JsonTokenType.StartArray:
            {
                var builder = ImmutableArray.CreateBuilder<string>();
            
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    builder.Add(reader.GetString()!);
                }
            
                return new PackageAuthors(string.Join(", ", builder), builder.ToImmutable());
            }
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException("String or array of strings expected");
        }
    }

    public override void Write(Utf8JsonWriter writer, PackageAuthors value, JsonSerializerOptions options)
    {
        if (value.Authors.Length == 1)
        {
            writer.WriteStringValue(value.Author);
            return;
        }
        
        writer.WriteStartArray();
        
        foreach (var author in value.Authors)
        {
            writer.WriteStringValue(author);
        }
        
        writer.WriteEndArray();
    }
}