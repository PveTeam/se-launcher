using System.Text.Json.Serialization;
using NuGet.Converters;
using NuGet.Frameworks;

namespace NuGet.Models;

/// <summary>
/// Represents a NuGetFramework with a runtime identifier
/// </summary>
[JsonConverter(typeof(RuntimeFrameworkJsonConverter))]
public record NuGetRuntimeFramework(NuGetFramework Framework, string? RuntimeIdentifier)
{
    public static NuGetRuntimeFramework Parse(string str)
    {
        var index = str.IndexOf('/');
        
        if (index < 0)
            return new NuGetRuntimeFramework(NuGetFramework.Parse(str), null);
        
        return new NuGetRuntimeFramework(NuGetFramework.Parse(str[..index]), str[(index + 1)..]);
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(RuntimeIdentifier) ? Framework.ToString() : $"{Framework}/{RuntimeIdentifier}";
    }
}