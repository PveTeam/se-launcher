using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text.Json;
using HarmonyLib;

namespace SharedCringe.Loader;

public class AlcMapper
{
    private readonly ConditionalWeakTable<AssemblyLoadContext, AlcMapping> _mapped = [];
    private readonly string _mapFilePath;
    private readonly Lock _mapFileLock = new();
    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web);
    
    private static AlcMapper? _instance;

    private AlcMapper(string mapFilePath)
    {
        _mapFilePath = mapFilePath;
    }

    public static void Initialize(string mapFilePath)
    {
        if (_instance is not null)
            throw new InvalidOperationException("AlcMapper already initialized");
        
        _instance = new(mapFilePath);
        
        Add(AssemblyLoadContext.Default);
    }

    public static void Add(AssemblyLoadContext alc) => _instance?._mapped.GetOrAdd(alc, _instance.MapAlc);

    private AlcMapping MapAlc(AssemblyLoadContext alc)
    {
        using var scope = _mapFileLock.EnterScope();

        var mapping = new AlcMapping(
            alc.Name ?? alc.ToString()!,
            alc.Id,
            alc.IsCollectible,
            alc.Handle.ToInt64(),
            alc.LoaderAllocatorHandle.ToInt64());

        using var stream = File.Open(_mapFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        JsonSerializer.Serialize(stream, mapping, _jsonSerializerOptions);
        stream.Write(OperatingSystem.IsWindows() ? "\r\n"u8 : "\n"u8);
        
        return mapping;
    }

    private record AlcMapping(string Name, long Id, bool IsCollectible, long Handle, long LoaderAllocatorHandle);
}

file static class AlcAccessorExtensions
{
    private static readonly AccessTools.FieldRef<AssemblyLoadContext, long> AlcId =
        AccessTools.FieldRefAccess<AssemblyLoadContext, long>("_id");
    
    private static readonly AccessTools.FieldRef<AssemblyLoadContext, nint> AlcHandle =
        AccessTools.FieldRefAccess<AssemblyLoadContext, nint>("_nativeAssemblyLoadContext");
    
    extension(AssemblyLoadContext alc)
    {
        public long Id => AlcId(alc);
        public nint Handle => AlcHandle(alc);

        public nint LoaderAllocatorHandle => GetLoaderAllocator(alc.Handle);
    }
    
    private static unsafe nint GetLoaderAllocator(nint assemblyBinder)
    {
        if (assemblyBinder == 0)
            return 0;

        const int getLoaderAllocatorSlot = 2;

        return ((delegate* unmanaged[SuppressGCTransition]<nint, nint>)(*(void***)assemblyBinder)[getLoaderAllocatorSlot])(assemblyBinder);
    }

}
