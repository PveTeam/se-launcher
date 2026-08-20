using System.Text.Json.Serialization;
using NuGet.Converters;
using NuGet.Versioning;

namespace NuGet.Models;

[JsonConverter(typeof(ResourceTypeJsonConverter))]
public record ResourceType(string Id, NuGetVersion? Version) : IComparable<ResourceType>
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

    public int CompareTo(ResourceType? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var idComparison = string.Compare(Id, other.Id, StringComparison.OrdinalIgnoreCase);
        if (idComparison != 0) return idComparison;
        return Comparer<SemanticVersion>.Default.Compare(Version, other.Version);
    }
}
