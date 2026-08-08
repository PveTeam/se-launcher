using System.Runtime.InteropServices;

namespace SharedCringe.Loader;

public partial class PeMapWriter
{
    public static void Write(string mapFilePath)
    {
        if (!OperatingSystem.IsLinux())
            return;

        try
        {
            var json = GetPeMapJson();
            if (string.IsNullOrEmpty(json))
                return;

            File.WriteAllText(mapFilePath, json);
        }
        catch (Exception e)
        {
            // Non-fatal: crash-report PE frame coloring is degraded without the map.
            Console.Error.WriteLine($"Unable to write pe map to {mapFilePath}: {e}");
        }
    }

    [LibraryImport("libCringeBootstrap.Native.so", EntryPoint = "CringeBootstrap_GetPeMapJson",
        StringMarshalling = StringMarshalling.Utf8)]
    private static partial string? GetPeMapJson();
}
