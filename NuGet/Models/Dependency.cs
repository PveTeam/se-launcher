using NuGet.Versioning;

namespace NuGet.Models;

public record Dependency(string Id, VersionRange Range);