using System.Collections.Immutable;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using NuGet.Converters;
using NuGet.Frameworks;
using NuGet.Models;
using NuGet.Versioning;

namespace NuGet.Deps;

public record DependenciesManifest(
    RuntimeTarget RuntimeTarget,
    ImmutableDictionary<NuGetRuntimeFramework, string> CompilationOptions,
    ImmutableDictionary<NuGetRuntimeFramework, ImmutableDictionary<ManifestPackageKey, DependencyTarget>> Targets,
    ImmutableDictionary<ManifestPackageKey, DependencyLibrary> Libraries);

public record DependencyLibrary(
    LibraryType Type,
    string Sha512 = "",
    bool Serviceable = false,
    ManifestPackageKey? Path = null,
    string? HashPath = null);

[JsonConverter(typeof(JsonStringEnumConverter<LibraryType>))]
public enum LibraryType
{
    Project,
    Package,
    Reference,
    Runtimepack
}

public record DependencyTarget(ImmutableDictionary<string, NuGetVersion>? Dependencies,
    // key is file path relative to package root
    ImmutableDictionary<string, RuntimeDependency>? Runtime,
    // key is file path relative to package root
    ImmutableDictionary<string, Dependency>? Native);

public record Dependency(Version? FileVersion = null);

public record RuntimeDependency(Version? AssemblyVersion = null, Version? FileVersion = null) : Dependency(FileVersion);

public record RuntimeTarget([property: JsonPropertyName("name")] NuGetRuntimeFramework RuntimeFramework, string Signature = "");

[JsonConverter(typeof(ManifestPackageKeyJsonConverter))]
public record ManifestPackageKey(string Id, NuGetVersion Version)
{
    public static ManifestPackageKey Parse(string str)
    {
        var index = str.IndexOf('/');
        if (index < 0)
            throw new FormatException("Invalid package key: " + str);

        return new ManifestPackageKey(str[..index], NuGetVersion.Parse(str[(index + 1)..]));
    }

    public override string ToString() => $"{Id}/{Version}";
}

public static class DependencyManifestSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters =
        {
            new FrameworkJsonConverter(FrameworkNameFormat.FrameworkName),
            new VersionJsonConverter()
        }
    };

    public static Task SerializeAsync(Stream stream, DependenciesManifest manifest) => JsonSerializer.SerializeAsync(stream, manifest, SerializerOptions);

    public static ValueTask<DependenciesManifest> DeserializeAsync(Stream stream) => JsonSerializer.DeserializeAsync<DependenciesManifest>(stream, SerializerOptions)!;
}

public class DependencyManifestBuilder(DirectoryInfo cacheDirectory, PackageSourceMapping packageSources, Func<Models.Dependency, CatalogEntry?> catalogEntryResolver)
{
    public async ValueTask WriteDependencyManifestAsync(Stream stream, CatalogEntry catalogEntry, NuGetRuntimeFramework targetFramework, Predicate<CatalogEntry>? dependencyLeafPredicate = null)
    {
        var runtimeTarget = new RuntimeTarget(targetFramework);

        var targets = ImmutableDictionary<ManifestPackageKey, DependencyTarget>.Empty.ToBuilder();
        var libraries = ImmutableDictionary<ManifestPackageKey, DependencyLibrary>.Empty.ToBuilder();

        await MapCatalogEntryAsync(catalogEntry, targetFramework, targets, libraries, dependencyLeafPredicate);

        var manifest = new DependenciesManifest(runtimeTarget, ImmutableDictionary<NuGetRuntimeFramework, string>.Empty,
            ImmutableDictionary<NuGetRuntimeFramework, ImmutableDictionary<ManifestPackageKey, DependencyTarget>>.Empty
                .Add(targetFramework, targets.ToImmutable()),
            libraries.ToImmutable());

        await DependencyManifestSerializer.SerializeAsync(stream, manifest);
    }

    private async Task MapCatalogEntryAsync(CatalogEntry catalogEntry, NuGetRuntimeFramework targetFramework,
        ImmutableDictionary<ManifestPackageKey, DependencyTarget>.Builder targets,
        ImmutableDictionary<ManifestPackageKey, DependencyLibrary>.Builder libraries,
        Predicate<CatalogEntry>? dependencyLeafPredicate)
    {
        var packageKey = new ManifestPackageKey(catalogEntry.Id, catalogEntry.Version);
        
        if (targets.ContainsKey(packageKey) || !catalogEntry.DependencyGroups.HasValue)
            return;

        // TODO take into account the target framework runtime identifier
        var nearest = NuGetFrameworkUtility.GetNearest(catalogEntry.DependencyGroups.Value, targetFramework.Framework,
            group => group.TargetFramework);

        if (nearest is null)
            return;

        targets.Add(packageKey,
            await MapEntryAsync(catalogEntry, nearest));

        libraries.Add(packageKey,
            new DependencyLibrary(LibraryType.Package, Serviceable: true, Path: packageKey));

        foreach (var entry in (nearest.Dependencies ?? []).Select(catalogEntryResolver))
        {
            // predicate is invoked lately so the root entry cannot be skipped
            if (entry is null || dependencyLeafPredicate?.Invoke(catalogEntry) is false)
                continue;

            await MapCatalogEntryAsync(entry, targetFramework, targets, libraries, dependencyLeafPredicate);
        }
    }

    private async Task<DependencyTarget> MapEntryAsync(CatalogEntry entry, DependencyGroup group)
    {
        var packageEntries = entry.PackageEntries ?? await GetPackageContent(entry);

        return new(
            group.Dependencies?.ToImmutableDictionary(b => b.Id, b => catalogEntryResolver(b)!.Version) ??
            ImmutableDictionary<string, NuGetVersion>.Empty,
            packageEntries.Where(b => b.FullName.StartsWith($@"lib\{group.TargetFramework.GetShortFolderName()}\") && 
                                      Path.GetExtension(b.FullName.AsSpan()) is ".dll")
                .ToImmutableDictionary(b => b.FullName.Replace('\\', '/'), _ => new RuntimeDependency()),
            packageEntries.Where(b =>
                    b.FullName.StartsWith($@"runtimes\{RuntimeInformation.RuntimeIdentifier}\native\"))
                .ToImmutableDictionary(b => b.FullName.Replace('\\', '/'), _ => new Dependency()));
    }

    private async Task<ImmutableArray<CatalogPackageEntry>> GetPackageContent(CatalogEntry entry)
    {
        while (true)
        {
            var dir = new DirectoryInfo(Path.Join(cacheDirectory.FullName, entry.Id, entry.Version.ToString()));

            if (dir.Exists)
            {
                return
                [
                    ..dir.EnumerateFiles("*", SearchOption.AllDirectories)
                        .Select(b => new CatalogPackageEntry(b.Name, Path.GetRelativePath(dir.FullName, b.FullName), b.Length, b.Length))
                ];
            }

            //don't call this method if client is null
            var client = await packageSources.GetClientAsync(entry.Id);

            dir.Create();

            {
                await using var stream = await client!.GetPackageContentStreamAsync(entry.Id, entry.Version);
                await using var archive = await ZipArchive.CreateAsync(stream, ZipArchiveMode.Read, true, null);
                await archive.ExtractToDirectoryAsync(dir.FullName);
            }
        }
    }
}