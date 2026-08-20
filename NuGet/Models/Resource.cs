using System.Text.Json.Serialization;

namespace NuGet.Models;

public record Resource(
    [property: JsonPropertyName("@id")] string Url,
    [property: JsonPropertyName("@type")] ResourceType Type,
    string? Comment) : IComparable<Resource>
{
    public int CompareTo(Resource? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Type.CompareTo(other.Type);
    }

    public override int GetHashCode() => Type.GetHashCode();

    public virtual bool Equals(Resource? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type.Equals(other.Type);
    }
}
