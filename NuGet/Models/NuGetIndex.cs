using NuGet.Versioning;

namespace NuGet.Models;

public record NuGetIndex(NuGetVersion Version, Resource[] Resources);