using System.Runtime.InteropServices;

namespace CringeLauncher.Utils;

internal static class PlatformApi
{
    private const string PlatformDllName = "CringeBootstrap.Native.so";
    private const string CallPrefix = "CringeBootstrap_";

    public static void CreateThread(ThreadStart start, string threadName)
    {
        Console.Error.WriteLine($"Platform Thread Request {threadName}");
        var startHandle = GCHandle.Alloc(start);
        var handle = PlatformCreateThread(0, () =>
        {
            Console.Error.WriteLine($"Platform Thread Proc {threadName}");
            startHandle.Free();
            start();
            GC.KeepAlive(start);
        }, threadName, threadName.Length);

        PlatformStartThread(handle);
    }

    [DllImport(PlatformDllName, EntryPoint = $"{CallPrefix}PlatformCreateThread", CharSet = CharSet.Unicode, PreserveSig = false, ExactSpelling = true)]
    private static extern nint PlatformCreateThread(nuint stackSize, ThreadStart start, string threadName, int threadNameLength);

    [DllImport(PlatformDllName, EntryPoint = $"{CallPrefix}PlatformStartThread", PreserveSig = false, ExactSpelling = true)]
    private static extern void PlatformStartThread(nint threadHandle);
}
