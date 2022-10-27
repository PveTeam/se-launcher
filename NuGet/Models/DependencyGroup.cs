using System.Collections.Immutable;
using NuGet.Frameworks;

namespace NuGet.Models;

public record DependencyGroup(NuGetFramework TargetFramework, ImmutableArray<Dependency>? Dependencies = null);