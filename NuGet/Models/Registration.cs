using System.Text.Json.Serialization;

namespace NuGet.Models;

public record Registration([property: JsonPropertyName("catalogEntry")] string CatalogEntryUrl, 
    [property: JsonPropertyName("sleet:catalogEntry")] CatalogEntry? SleetEntry);