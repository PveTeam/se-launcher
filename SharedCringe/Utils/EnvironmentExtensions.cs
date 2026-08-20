using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace SharedCringe.Utils;

internal static partial class EnvironmentExtensions
{
    extension(Environment)
    {
        public static void SetEnvironmentVariableNoCap(string variable, string? value)
        {
            if (!OperatingSystem.IsWindows())
            {
                if (SetEnvironmentVariable(variable, value, true) != 0)
                    throw new Win32Exception(Marshal.GetLastPInvokeError(), Marshal.GetLastPInvokeErrorMessage());
            }
            
            Environment.SetEnvironmentVariable(variable, value);
        }
    }
    
    [LibraryImport("c", StringMarshalling = StringMarshalling.Utf8, EntryPoint = "setenv", SetLastError = true)]
    [UnsupportedOSPlatform("windows")]
    private static partial int SetEnvironmentVariable(string name, string? value, [MarshalAs(UnmanagedType.Bool)] bool overwrite);
}
