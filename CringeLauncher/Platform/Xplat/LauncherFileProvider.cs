using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using VRage.FileSystem;

namespace CringeLauncher.Platform.Xplat;

public class LauncherFileProvider : IFileProvider
{
    private FrozenSet<string>? _cachedFiles;
    private FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> _cacheLookup;
    private HashSet<string>.AlternateLookup<ReadOnlySpan<char>> _modCacheLookup;
    private string? _modPathPrefix;

    public static readonly LauncherFileProvider Instance = new();
    
    public Stream? Open(string path, FileMode mode, FileAccess access, FileShare share)
    {
        NormalizePath(ref path);

        if (!File.Exists(path)) return null;

        try
        {
            return File.Open(path, mode, access, share);
        }
        catch (IOException)
        {
            return null;
        }
    }

    public bool DirectoryExists(string path)
    {
        NormalizePath(ref path);

        return Directory.Exists(path);
    }

    public IEnumerable<string> GetFiles(string path, string filter, MySearchOption searchOption)
    {
        NormalizePath(ref path);

        if (!Directory.Exists(path))
            return [];

        return Directory.EnumerateFiles(path, filter,
            searchOption == MySearchOption.TopDirectoryOnly
                ? SearchOption.TopDirectoryOnly
                : SearchOption.AllDirectories);
    }

    public bool FileExists(string path)
    {
        NormalizePath(ref path);
        
        return File.Exists(path);
    }

    public void NormalizePath(ref string path)
    {
        path = path.Replace('\\', Path.DirectorySeparatorChar);

        if (!path.StartsWith(MyFileSystem.ContentPath, StringComparison.OrdinalIgnoreCase))
        {
            if (_modPathPrefix is not null && path.StartsWith(_modPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                if (!_modCacheLookup.TryGetValue(Path.GetFullPath(path).AsSpan((_modPathPrefix.Length + 1)..), out var modCachedPath))
                    return;
                path = Path.Join(_modPathPrefix, modCachedPath);
            }
            
            return;
        }

        if (path.Length == MyFileSystem.ContentPath.Length) return;

        if (_cachedFiles is null) CreateCache();
            
        if (!_cacheLookup.TryGetValue(path.AsSpan((MyFileSystem.ContentPath.Length + 1)..), out var cachedPath))
            return;

        path = Path.Join(MyFileSystem.ContentPath, cachedPath);
    }

    [MemberNotNull(nameof(_cachedFiles))]
    private void CreateCache()
    {
        var files = Directory.GetFiles(MyFileSystem.ContentPath, "*", SearchOption.AllDirectories);
        
        for (var i = 0; i < files.Length; i++)
        {
            files[i] = files[i][(MyFileSystem.ContentPath.Length + 1)..];
        }
        
        _cachedFiles = FrozenSet.Create(StringComparer.OrdinalIgnoreCase, files);
        _cacheLookup = _cachedFiles.GetAlternateLookup<ReadOnlySpan<char>>();
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static string? GetFileName(string? path)
    {
        if (path == null)
            return null;

        Span<char> pathSpan = stackalloc char[path.Length];
        path.AsSpan().CopyTo(pathSpan);
        pathSpan.Replace('\\', Path.DirectorySeparatorChar);
        var result = Path.GetFileName(pathSpan);
        
        return path.Length == result.Length ? path : result.ToString();
    }

    [return: NotNullIfNotNull(nameof(path))]
    public static string? GetDirectoryName(string? path)
    {
        if (path == null)
            return null;

        Span<char> pathSpan = stackalloc char[path.Length];
        path.AsSpan().CopyTo(pathSpan);
        pathSpan.Replace('\\', Path.DirectorySeparatorChar);
        return Path.GetDirectoryName(pathSpan).ToString();
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public void CacheMods(IEnumerable<string> mods)
    {
        _modPathPrefix = null;
        _modCacheLookup = default;
        
        var first = mods.FirstOrDefault();
        if (first is null) return;

        _modPathPrefix = Path.GetDirectoryName(first)!;

        var files = new List<string>();
        
        foreach (var mod in mods)
        {
            files.AddRange(Directory.GetFiles(mod, "*", SearchOption.AllDirectories));
        }
        
        for (var i = 0; i < files.Count; i++)
        {
            files[i] = files[i][(_modPathPrefix.Length + 1)..];
        }
        
        _modCacheLookup = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase).GetAlternateLookup<ReadOnlySpan<char>>();
    }
}
