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

public record DependenciesManifest(RuntimeTarget RuntimeTarget, 
    ImmutableDictionary<NuGetFramework, string> CompilationOptions,
    ImmutableDictionary<NuGetFramework, ImmutableDictionary<ManifestPackageKey, DependencyTarget>> Targets,
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
    Package
}

public record DependencyTarget(ImmutableDictionary<string, NuGetVersion>? Dependencies,
    // key is file path relative to package root
    ImmutableDictionary<string, RuntimeDependency>? Runtime,
    // key is file path relative to package root
    ImmutableDictionary<string, Dependency>? Native);

public record Dependency(Version? FileVersion = null);

public record RuntimeDependency(Version? AssemblyVersion = null, Version? FileVersion = null) : Dependency(FileVersion);

public record RuntimeTarget([property: JsonPropertyName("name")] NuGetFramework Framework, string Signature = "");

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

public class DependencyManifestBuilder(DirectoryInfo cacheDirectory, PackageSourceMapping packageSources, Func<Models.Dependency, NuGetVersion?> versionResolver)
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

    public async ValueTask WriteDependencyManifestAsync(Stream stream, CatalogEntry catalogEntry, NuGetFramework targetFramework)
    {
        var runtimeTarget = new RuntimeTarget(targetFramework);

        var targets = ImmutableDictionary<ManifestPackageKey, DependencyTarget>.Empty.ToBuilder();

        await MapCatalogEntryAsync(catalogEntry, targetFramework, targets);
        
        var manifest = new DependenciesManifest(runtimeTarget, ImmutableDictionary<NuGetFramework, string>.Empty,
            ImmutableDictionary<NuGetFramework, ImmutableDictionary<ManifestPackageKey, DependencyTarget>>.Empty
                .Add(targetFramework, targets.ToImmutable()), 
            ImmutableDictionary<ManifestPackageKey, DependencyLibrary>.Empty);
        
        await JsonSerializer.SerializeAsync(stream, manifest, SerializerOptions);
    }

    private async Task MapCatalogEntryAsync(CatalogEntry catalogEntry, NuGetFramework targetFramework,
        ImmutableDictionary<ManifestPackageKey, DependencyTarget>.Builder targets)
    {
        if (targets.ContainsKey(new(catalogEntry.Id, catalogEntry.Version)) || !catalogEntry.DependencyGroups.HasValue)
            return;
        
        var nearest = NuGetFrameworkUtility.GetNearest(catalogEntry.DependencyGroups.Value, targetFramework,
            group => group.TargetFramework);

        if (nearest is null)
            return;
        
        targets.Add(new(catalogEntry.Id, catalogEntry.Version),
            await MapEntryAsync(catalogEntry, nearest));

        foreach (var dependency in nearest.Dependencies ?? [])
        {
            var client = await packageSources.GetClientAsync(dependency.Id);
            var registrationRoot = await client.GetPackageRegistrationRootAsync(dependency.Id);

            var version = versionResolver(dependency)!;
            var entry = registrationRoot.Items.SelectMany(b => b.Items ?? []).FirstOrDefault(b => b.CatalogEntry.Version == version)?.CatalogEntry;

            if (entry is null)
            {
                var (url, sleetEntry) = await client.GetPackageRegistrationAsync(dependency.Id, versionResolver(dependency)!);

                entry = sleetEntry;
                entry ??= await client.GetPackageCatalogEntryAsync(url);
            }

            await MapCatalogEntryAsync(entry, targetFramework, targets);
        }
    }

    private async Task<DependencyTarget> MapEntryAsync(CatalogEntry entry, DependencyGroup group)
    {
        var packageEntries = entry.PackageEntries ?? await GetPackageContent(entry);
        
        return new(group.Dependencies?.ToImmutableDictionary(b => b.Id, versionResolver) ?? ImmutableDictionary<string, NuGetVersion>.Empty,
            packageEntries.Where(b => b.FullName.StartsWith($"lib/{group.TargetFramework.GetShortFolderName()}/"))
                .ToImmutableDictionary(b => b.FullName, _ => new RuntimeDependency()),
            packageEntries.Where(b =>
                    b.FullName.StartsWith($"runtimes/{RuntimeInformation.RuntimeIdentifier}/native/"))
                .ToImmutableDictionary(b => b.FullName, _ => new Dependency()));
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
                        .Select(b => new CatalogPackageEntry(b.Name, b.FullName, b.Length, b.Length))
                ];
            }

            var client = await packageSources.GetClientAsync(entry.Id);

            dir.Create();

            {
                await using var stream = await client.GetPackageContentStreamAsync(entry.Id, entry.Version);
                using var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;
                using var archive = new ZipArchive(memStream, ZipArchiveMode.Read);
                archive.ExtractToDirectory(dir.FullName);
            }
        }
    }
}