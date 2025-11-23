using System.Diagnostics;
using Windows.Win32;

namespace CringeLauncher.Utils;

internal static class ConsoleHandler
{
    public static bool ShouldKeepConsole(string[] args)
    {
        return Debugger.IsAttached || args.Contains("--keep-console", StringComparer.OrdinalIgnoreCase);
    }

    public static void FreeConsole()
    {
        Console.SetOut(new StreamWriter(Stream.Null));
        Console.SetError(new StreamWriter(Stream.Null));
        Console.SetIn(new StreamReader(Stream.Null));
#if WINDOWS
        PInvoke.FreeConsole();
#endif
    }
}
