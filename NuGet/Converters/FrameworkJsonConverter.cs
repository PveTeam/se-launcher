using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Frameworks;

namespace NuGet.Converters;

public class FrameworkJsonConverter(FrameworkNameFormat format) : JsonConverter<NuGetFramework>
{
    public override NuGetFramework Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Invalid framework string");

        var s = reader.GetString()!;
        return format switch
        {
            FrameworkNameFormat.ShortFolderName => NuGetFramework.ParseFolder(s),
            FrameworkNameFormat.FrameworkName => NuGetFramework.ParseFrameworkName(s,
                DefaultFrameworkNameProvider.Instance),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override void Write(Utf8JsonWriter writer, NuGetFramework value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(FormatValue(value));
    }

    private string FormatValue(NuGetFramework value)
    {
        return format switch
        {
            FrameworkNameFormat.ShortFolderName => value.GetShortFolderName(),
            FrameworkNameFormat.FrameworkName => value.DotNetFrameworkName,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public override NuGetFramework ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options) => Read(ref reader, typeToConvert, options);

    public override void WriteAsPropertyName(Utf8JsonWriter writer, NuGetFramework value,
        JsonSerializerOptions options)
    {
        writer.WritePropertyName(FormatValue(value));
    }
}

public enum FrameworkNameFormat
{
    /// <summary>
    /// The short folder name format (net8.0)
    /// </summary>
    ShortFolderName,
    /// <summary>
    /// Full framework name (.NETCoreApp,Version=v8.0)
    /// </summary>
    FrameworkName
}