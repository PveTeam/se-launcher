using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using VRage.FileSystem;

namespace CringeLauncher.Platform.Xplat;

public class LauncherFileProvider : IFileProvider
{
    private FrozenSet<string>? _cachedFiles;
    private FrozenSet<string>.AlternateLookup<ReadOnlySpan<char>> _cacheLookup;

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

        if (!path.StartsWith(MyFileSystem.ContentPath, StringComparison.Ordinal)) return;
        
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
}
