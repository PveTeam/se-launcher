using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.System.Console;
using NLog;

namespace CringeLauncher.Utils;

internal static class ConsoleHandler
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public static bool ShouldKeepConsole(string[] args)
    {
        return Debugger.IsAttached || args.Contains("--keep-console", StringComparer.OrdinalIgnoreCase);
    }

    public static void FreeConsole()
    {
        Console.SetOut(new StreamWriter(Stream.Null));
        Console.SetError(new StreamWriter(Stream.Null));
        Console.SetIn(new StreamReader(Stream.Null));
        PInvoke.FreeConsole();
    }

    public static void RedirectStandardError(string redirectPath)
    {
        var standardError = PInvoke.CrtGetStdHandle(CrtStdHandle.ErrorHandle);
        if (PInvoke.CrtReopenFile(out var redirectFile, redirectPath, "w", standardError) != 0)
        {
            Log.Error("Failed to reopen stderr handle");
            return;
        }

        var handle = PInvoke.CrtGetOsFileHandle(PInvoke.CrtGetFileDescriptor(redirectFile));
        PInvoke.SetStdHandle(STD_HANDLE.STD_ERROR_HANDLE, handle);
    }
}