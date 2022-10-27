using System.Text.Json.Serialization;

namespace NuGet.Models;

public record Resource([property: JsonPropertyName("@id")] string Url, [property: JsonPropertyName("@type")] ResourceType Type, string? Comment);