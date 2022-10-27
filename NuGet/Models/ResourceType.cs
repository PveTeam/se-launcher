using System.Text.Json.Serialization;
using NuGet.Converters;
using NuGet.Versioning;

namespace NuGet.Models;

[JsonConverter(typeof(ResourceTypeJsonConverter))]
public record ResourceType(string Id, NuGetVersion? Version)
{
    public static ResourceType Parse(string typeString)
    {
        var slash = typeString.IndexOf('/');
        
        if (slash < 0)
            return new ResourceType(typeString, null);
        
        var id = typeString[..slash];
        var versionStr = typeString[(slash + 1)..];

        return NuGetVersion.TryParse(versionStr, out var version)
            ? new ResourceType(id, version)
            : new ResourceType(id, null);
    }

    public override string ToString() => $"{Id}/{Version}";
}