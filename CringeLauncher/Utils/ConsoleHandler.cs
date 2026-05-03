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
#if WINDOWS
        PInvoke.FreeConsole();
#endif
    }

    public static void RedirectStandardError(string redirectPath)
    {
#if WINDOWS
        var standardError = PInvoke.CrtGetStdHandle(CrtStdHandle.ErrorHandle);
        if (PInvoke.CrtReopenFile(out var redirectFile, redirectPath, "w", standardError) != 0)
        {
            Log.Error("Failed to reopen stderr handle");
            return;
        }

        var handle = PInvoke.CrtGetOsFileHandle(PInvoke.CrtGetFileDescriptor(redirectFile));
        PInvoke.SetStdHandle(STD_HANDLE.STD_ERROR_HANDLE, handle);
#else
        var fd = PInvoke.Open(redirectPath, PInvoke.O_WRONLY | PInvoke.O_CREAT | PInvoke.O_TRUNC, 644);
        if (fd < 0)
        {
            Log.Error(PInvoke.GetExceptionForLastError(), "Failed to open stderr redirect file");
            return;
        }
        
        if (PInvoke.Dup2(fd, PInvoke.STDERR_FILENO) < 0)
        {
            Log.Error(PInvoke.GetExceptionForLastError(), "Failed to duplicate stderr file descriptor");
        }
        
        if (PInvoke.Close(fd) < 0)
        {
            Log.Error(PInvoke.GetExceptionForLastError(), "Failed to close stderr file descriptor");
        }
        
        // Console.SetError(
        //     new StreamWriter(new FileStream(redirectPath, FileMode.Open, FileAccess.Write, FileShare.Write))
        //         { AutoFlush = true });
#endif
    }
}
