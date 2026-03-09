using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CringeLauncher.Utils;

internal static class PlatformApi
{
    internal const string PlatformDllName = "CringeBootstrap.Native.so";
    internal const string CallPrefix = "CringeBootstrap_";

    public static void CreateThread(ThreadStart start, string threadName)
    {
        var box = new StrongBox<GCHandle>();
        ThreadStart threadStart = () =>
        {
            Thread.CurrentThread.Name = threadName;
            start();
            if (box.Value.IsAllocated) box.Value.Free();
        };
        box.Value = GCHandle.Alloc(threadStart);
        var handle = PlatformCreateThread(8 * 1024 * 1024, threadStart, threadName, threadName.Length);

        PlatformStartThread(handle);
    }

    [DllImport(PlatformDllName, EntryPoint = $"{CallPrefix}PlatformCreateThread", CharSet = CharSet.Ansi, PreserveSig = false, ExactSpelling = true)]
    private static extern nint PlatformCreateThread(nuint stackSize, ThreadStart start, string threadName, int threadNameLength);

    [DllImport(PlatformDllName, EntryPoint = $"{CallPrefix}PlatformStartThread", PreserveSig = false, ExactSpelling = true)]
    private static extern void PlatformStartThread(nint threadHandle);
}
