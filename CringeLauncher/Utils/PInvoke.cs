// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Windows.Win32.Foundation;
using Windows.Win32.System.Console;

namespace Windows.Win32
{
    internal partial class PInvoke
    {
#if WINDOWS
        private const string StdIoDll = "api-ms-win-crt-stdio-l1-1-0.dll";

        [LibraryImport(StdIoDll, EntryPoint = "__acrt_iob_func")]
        public static partial FILE CrtGetStdHandle(CrtStdHandle stdHandle);

        [LibraryImport(StdIoDll, EntryPoint = "_wfreopen_s", StringMarshalling = StringMarshalling.Utf16)]
        public static partial int CrtReopenFile(out FILE stream, string fileName, string mode, FILE oldStream);

        [LibraryImport(StdIoDll, EntryPoint = "_fileno")]
        public static partial int CrtGetFileDescriptor(FILE stream);

        [LibraryImport(StdIoDll, EntryPoint = "_get_osfhandle")]
        [return: MarshalUsing(typeof(HandleMarshaller))]
        public static partial HANDLE CrtGetOsFileHandle(int fd);
#else
        public const int O_WRONLY = 0x01;
        public const int O_CREAT = 0x40;
        public const int O_TRUNC = 0x200;
        
        public const int STDERR_FILENO = 2;
        private const string LibCName = "c";

        [LibraryImport(LibCName, EntryPoint = "open", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        public static partial int Open(string pathname, int flags, int mode);

        public static int Open(string pathname, int flags, Permissions mode) => Open(pathname, flags, (int)mode);

        [LibraryImport(LibCName, EntryPoint = "dup2", SetLastError = true)]
        public static partial int Dup2(int oldfd, int newfd);

        [LibraryImport(LibCName, EntryPoint = "close", SetLastError = true)]
        public static partial int Close(int fd);

        [LibraryImport(LibCName, EntryPoint = "strerror", StringMarshalling = StringMarshalling.Utf8)]
        private static partial string StrError(int errnum);
        
        public static Exception? GetExceptionForLastError()
        {
            var error = Marshal.GetLastWin32Error();
            return new Win32Exception(error, StrError(error));
        }
#endif
    }

    namespace Foundation
    {
        [NativeMarshalling(typeof(FileHandleMarshaller))]
        internal readonly struct FILE(nint value)
        {
            private readonly nint _value = value;

            public static implicit operator nint(FILE file) => file._value;
            public static implicit operator FILE(nint file) => new(file);
        }

        [CustomMarshaller(typeof(FILE), MarshalMode.Default, typeof(FileHandleMarshaller))]
        internal static class FileHandleMarshaller
        {
            public static nint ConvertToUnmanaged(FILE managed) => managed;
            public static FILE ConvertToManaged(nint unmanaged) => unmanaged;
        }
        
        [CustomMarshaller(typeof(HANDLE), MarshalMode.Default, typeof(HandleMarshaller))]
        internal static class HandleMarshaller
        {
            public static nint ConvertToUnmanaged(HANDLE managed) => managed;
            public static HANDLE ConvertToManaged(nint unmanaged) => (HANDLE)unmanaged;
        }
        
#if !WINDOWS
        [Flags]
        public enum Permissions
        {
            None = 0,
            OwnerRead = 0400,  // S_IRUSR
            OwnerWrite = 0200, // S_IWUSR
            OwnerExec = 0100,  // S_IXUSR
            OwnerAll = 0700,   // S_IRWXU
            GroupRead = 040,   // S_IRGRP
            GroupWrite = 020,  // S_IWGRP
            GroupExec = 010,   // S_IXGRP
            GroupAll = 070,    // S_IRWXG
            OthersRead = 04,   // S_IROTH
            OthersWrite = 02,  // S_IWOTH
            OthersExec = 01,   // S_IXOTH
            OthersAll = 07,    // S_IRWXO
            All = 0777,
            SetUid = 04000,    // S_ISUID
            SetGid = 02000,    // S_ISGID
            StickyBit = 01000, // S_ISVTX
            Mask = 07777,
            Unknown = 0xFFFF,
            AddPerms = 0x10000,
            RemovePerms = 0x20000,
            ResolveSymlinks = 0x40000
        }
#endif
    }
    
    namespace System.Console
    {
        internal enum CrtStdHandle : uint
        {
            InputHandle,
            OutputHandle,
            ErrorHandle
        }
    }
}
