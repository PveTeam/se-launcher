using System.Text.Json.Serialization;
using NuGet.Versioning;

namespace NuGet.Models;

public record RegistrationPage([property: JsonPropertyName("@id")] string Url, int Count, NuGetVersion Lower,
    NuGetVersion Upper, RegistrationEntry[]? Items);