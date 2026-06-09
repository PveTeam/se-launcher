using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace CringeLauncher.Platform.Xplat;

// ====================================================================================
// Constants (code pages, FOURCC, DXC_ARG_*)
// ====================================================================================

/// <summary>Constants used by the DirectX Compiler API.</summary>
public static class DxcConstants
{
    // Code pages
    /// <summary>UTF-8 code page (65001).</summary>
    public const uint CP_UTF8 = 65001;

    /// <summary>UTF-16 code page (1200).</summary>
    public const uint CP_UTF16 = 1200;

    /// <summary>UTF-32 code page (12000).</summary>
    public const uint CP_UTF32 = 12000;

    /// <summary>ANSI code page or binary (0).</summary>
    public const uint CP_ACP = 0;

    // DXIL container part identifiers
    /// <summary>PDB part identifier.</summary>
    public const uint PartPDB = 0x494C4442;

    /// <summary>PDB name part identifier.</summary>
    public const uint PartPDBName = 0x494C444E;

    /// <summary>Private data part identifier.</summary>
    public const uint PartPrivateData = 0x50524956;

    /// <summary>Root signature part identifier.</summary>
    public const uint PartRootSignature = 0x52545330;

    /// <summary>DXIL part identifier.</summary>
    public const uint PartDXIL = 0x4458494C;

    /// <summary>Reflection data part identifier.</summary>
    public const uint PartReflectionData = 0x53544154;

    /// <summary>Shader hash part identifier.</summary>
    public const uint PartShaderHash = 0x48415348;

    /// <summary>Input signature part identifier.</summary>
    public const uint PartInputSignature = 0x49534731;

    /// <summary>Output signature part identifier.</summary>
    public const uint PartOutputSignature = 0x4F534731;

    /// <summary>Patch constant signature part identifier.</summary>
    public const uint PartPatchConstantSignature = 0x50534731;

    // DXC_ARG_* definitions (compiler arguments)
    /// <summary>Debug information flag.</summary>
    public const string ArgDebug = "-Zi";

    /// <summary>Skip validation flag.</summary>
    public const string ArgSkipValidation = "-Vd";

    /// <summary>Skip optimizations flag.</summary>
    public const string ArgSkipOptimizations = "-Od";

    /// <summary>Pack matrices in row-major order.</summary>
    public const string ArgPackMatrixRowMajor = "-Zpr";

    /// <summary>Pack matrices in column-major order.</summary>
    public const string ArgPackMatrixColumnMajor = "-Zpc";

    /// <summary>Avoid flow control flag.</summary>
    public const string ArgAvoidFlowControl = "-Gfa";

    /// <summary>Prefer flow control flag.</summary>
    public const string ArgPreferFlowControl = "-Gfp";

    /// <summary>Enable strictness flag.</summary>
    public const string ArgEnableStrictness = "-Ges";

    /// <summary>Enable backwards compatibility flag.</summary>
    public const string ArgEnableBackwardsCompatibility = "-Gec";

    /// <summary>IEEE strictness flag.</summary>
    public const string ArgIeeeStrictness = "-Gis";

    /// <summary>Optimization level 0 (disabled).</summary>
    public const string ArgOptimizationLevel0 = "-O0";

    /// <summary>Optimization level 1.</summary>
    public const string ArgOptimizationLevel1 = "-O1";

    /// <summary>Optimization level 2.</summary>
    public const string ArgOptimizationLevel2 = "-O2";

    /// <summary>Optimization level 3.</summary>
    public const string ArgOptimizationLevel3 = "-O3";

    /// <summary>Treat warnings as errors.</summary>
    public const string ArgWarningsAreErrors = "-WX";

    /// <summary>Resources may alias flag.</summary>
    public const string ArgResourcesMayAlias = "-res_may_alias";

    /// <summary>All resources bound flag.</summary>
    public const string ArgAllResourcesBound = "-all_resources_bound";

    /// <summary>Debug name for source (include source in hash).</summary>
    public const string ArgDebugNameForSource = "-Zss";

    /// <summary>Debug name for binary.</summary>
    public const string ArgDebugNameForBinary = "-Zsb";
}

[Flags]
public enum DxcValidatorFlags : uint
{
    /// <summary>Default validator flags.</summary>
    Default = 0,

    /// <summary>Allow in-place edit of shader blob.</summary>
    InPlaceEdit = 1,

    /// <summary>Validate root signature only.</summary>
    RootSignatureOnly = 2,

    /// <summary>Validate module only.</summary>
    ModuleOnly = 4,
}

// ====================================================================================
// Enums (with simplified member names)
// ====================================================================================

/// <summary>Types of output that can be retrieved from an <see cref="IDxcResult"/>.</summary>
public enum DxcOutputKind : uint
{
    /// <summary>No output.</summary>
    None = 0,

    /// <summary>Shader or library object (IDxcBlob).</summary>
    Object = 1,

    /// <summary>Error messages (IDxcBlobUtf8 or IDxcBlobWide).</summary>
    Errors = 2,

    /// <summary>Program database (IDxcBlob).</summary>
    Pdb = 3,

    /// <summary>Shader hash (IDxcBlob containing DxcShaderHash).</summary>
    ShaderHash = 4,

    /// <summary>Disassembly text (IDxcBlobUtf8 or IDxcBlobWide).</summary>
    Disassembly = 5,

    /// <summary>Preprocessed HLSL (IDxcBlobUtf8 or IDxcBlobWide).</summary>
    Hlsl = 6,

    /// <summary>Other text output, e.g. -ast-dump (IDxcBlobUtf8 or IDxcBlobWide).</summary>
    Text = 7,

    /// <summary>Reflection data (IDxcBlob).</summary>
    Reflection = 8,

    /// <summary>Serialized root signature (IDxcBlob).</summary>
    RootSignature = 9,

    /// <summary>Extra outputs (IDxcExtraOutputs).</summary>
    ExtraOutputs = 10,

    /// <summary>Remarks (IDxcBlobUtf8 or IDxcBlobWide).</summary>
    Remarks = 11,

    /// <summary>Time report (IDxcBlobUtf8 or IDxcBlobWide).</summary>
    TimeReport = 12,

    /// <summary>Time trace (IDxcBlobUtf8 or IDxcBlobWide).</summary>
    TimeTrace = 13,
}

// ====================================================================================
// Structs
// ====================================================================================

/// <summary>Buffer structure for passing data into DXC APIs.</summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct DxcBuffer
{
    /// <summary>Pointer to the start of the buffer.</summary>
    public IntPtr Ptr;

    /// <summary>Size of the buffer in bytes.</summary>
    public UIntPtr Size;

    /// <summary>Encoding of the buffer (use 0 for binary or unknown).</summary>
    public uint Encoding;
}

// ====================================================================================
// Custom marshaller for platform‑dependent wide strings (wchar_t*)
// ====================================================================================

/// <summary>Provides marshaling for strings to/from native wchar_t* using the correct
/// character width for the target platform (UTF‑16 on Windows, UTF‑32 on others).</summary>
[CustomMarshaller(typeof(string), MarshalMode.Default, typeof(WideStringMarshaller))]
[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedIn, typeof(WideStringMarshaller))]
public static class WideStringMarshaller
{
    public static readonly Encoding PlatformEncoding = OperatingSystem.IsWindows()
        ? Encoding.Unicode
        : // UTF-16, 2 bytes per char
        Encoding.UTF32; // UTF-32, 4 bytes per char

    public static readonly int ElementSize = OperatingSystem.IsWindows() ? 2 : 4;

    /// <summary>Converts a managed string to a native wchar_t*.</summary>
    /// <param name="managed">The managed string to convert.</param>
    /// <returns>A pointer to a null‑terminated native string, or <see cref="IntPtr.Zero"/> if <paramref name="managed"/> is null.</returns>
    public static IntPtr ConvertToUnmanaged(string? managed)
    {
        if (managed is null)
            return IntPtr.Zero;

        var byteCount = PlatformEncoding.GetByteCount(managed);
        // Add one character for null terminator
        var totalBytes = byteCount + ElementSize;

        var ptr = Marshal.AllocCoTaskMem(totalBytes);
        unsafe
        {
            var bytes = new Span<byte>((void*)ptr, totalBytes);
            var written = PlatformEncoding.GetBytes(managed, bytes);
            // Write null terminator
            bytes[written..].Clear();
        }

        return ptr;
    }

    /// <summary>Converts a native wchar_t* to a managed string.</summary>
    /// <param name="unmanaged">The native pointer to a null‑terminated string.</param>
    /// <returns>A managed string, or null if <paramref name="unmanaged"/> is <see cref="IntPtr.Zero"/>.</returns>
    public static string? ConvertToManaged(IntPtr unmanaged)
    {
        if (unmanaged == IntPtr.Zero)
            return null;

        var strLen = 0;
        unsafe
        {
            if (ElementSize == 2)
            {
                var ptr = (ushort*)unmanaged;
                while (ptr[strLen] != '\0')
                    strLen++;
            }
            else
            {
                var ptr = (uint*)unmanaged;
                while (ptr[strLen] != '\0')
                    strLen++;
            }

            return PlatformEncoding.GetString(new Span<byte>((void*)unmanaged, strLen * ElementSize));
        }
    }

    /// <summary>Frees the native memory allocated for the string.</summary>
    /// <param name="unmanaged">The native pointer to free.</param>
    public static void Free(IntPtr unmanaged) => Marshal.FreeCoTaskMem(unmanaged);
}

// ====================================================================================
// COM Interface Definitions (GeneratedComInterface, PreserveSig)
// ====================================================================================

/// <summary>Basic blob interface representing a sized buffer.</summary>
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper), Guid("8BA5FB08-5195-40e2-AC58-0D989C3A0102")]
public partial interface IDxcBlob
{
    /// <summary>Retrieves a pointer to the blob's data.</summary>
    [PreserveSig]
    nint GetBufferPointer();

    /// <summary>Retrieves the size of the blob in bytes.</summary>
    [PreserveSig]
    nuint GetBufferSize();
}

/// <summary>Blob that may have a known text encoding.</summary>
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper), Guid("7241d424-2646-4191-97c0-98e96e42fc68")]
public partial interface IDxcBlobEncoding : IDxcBlob
{
    /// <summary>Returns whether the encoding is known and its code page.</summary>
    [PreserveSig]
    int GetEncoding([MarshalAs(UnmanagedType.I1)] out bool pKnown, out uint pCodePage);
}

/// <summary>Blob containing a null‑terminated wide string (platform‑dependent width).</summary>
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper), Guid("A3F84EAB-0FAA-497E-A39C-EE6ED60B2D84")]
public partial interface IDxcBlobWide : IDxcBlobEncoding
{
    /// <summary>Returns a pointer to the string data.</summary>
    [PreserveSig]
    nint GetStringPointer();

    /// <summary>Returns the length of the string in characters (excluding null terminator).</summary>
    [PreserveSig]
    nuint GetStringLength();
}

/// <summary>Blob containing a UTF‑8 encoded string.</summary>
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper), Guid("3DA636C9-BA71-4024-A301-30CBF125305B")]
public partial interface IDxcBlobUtf8 : IDxcBlobEncoding
{
    /// <summary>Returns a pointer to the UTF‑8 string data.</summary>
    [PreserveSig]
    nint GetStringPointer();

    /// <summary>Returns the length of the string in characters (excluding null terminator).</summary>
    [PreserveSig]
    nuint GetStringLength();
}

/// <summary>Handler for #include directives.</summary>
[GeneratedComInterface(StringMarshallingCustomType = typeof(WideStringMarshaller)),
 Guid("7f61fc7d-950d-467f-b3e3-3c02fb49187c")]
public partial interface IDxcIncludeHandler
{
    /// <summary>Loads a source file to be included.</summary>
    /// <param name="pFilename">Candidate filename.</param>
    /// <returns>Receives the loaded blob, or null if not found.</returns>
    IDxcBlob? LoadSource(string pFilename);
}

/// <summary>Result of a DXC operation (compilation, disassembly, etc.).</summary>
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper), Guid("58346CDA-DDE7-4497-9461-6F87AF5E0659")]
public partial interface IDxcResult : IDxcOperationResult
{
    /// <summary>Checks whether the result contains a given output kind.</summary>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.I1)]
    bool HasOutput(DxcOutputKind dxcOutKind);

    /// <summary>Retrieves the specified output.</summary>
    [PreserveSig]
    int GetOutput(DxcOutputKind dxcOutKind, in Guid riid, out IntPtr ppvObject, out IDxcBlobWide? ppOutputName);

    /// <summary>Returns the number of outputs available.</summary>
    [PreserveSig]
    uint GetNumOutputs();

    /// <summary>Returns the output kind at the given index.</summary>
    [PreserveSig]
    DxcOutputKind GetOutputByIndex(uint index);

    /// <summary>Returns the primary output kind for this result.</summary>
    [PreserveSig]
    DxcOutputKind PrimaryOutput();
}

/// <summary>Legacy operation result interface (still used by some methods).</summary>
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper), Guid("CEDB484A-D4E9-445A-B991-CA21CA157DC2")]
public partial interface IDxcOperationResult
{
    [PreserveSig]
    int GetStatus(out int pStatus);

    [PreserveSig]
    int GetResult(out IDxcBlob? ppResult);

    [PreserveSig]
    int GetErrorBuffer(out IDxcBlobEncoding? ppErrors);
}

/// <summary>Utility functions (blob creation, file loading, include handler, etc.).</summary>
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper, StringMarshallingCustomType = typeof(WideStringMarshaller)), Guid("4605C4CB-2019-492A-ADA4-65F20BB7D67F")]
public partial interface IDxcUtils
{
    [PreserveSig]
    int CreateBlobFromBlob(IDxcBlob pBlob, uint offset, uint length, out IDxcBlob ppResult);

    [PreserveSig]
    int CreateBlobFromPinned(IntPtr pData, uint size, uint codePage, out IDxcBlobEncoding ppBlobEncoding);

    [PreserveSig]
    int MoveToBlob(IntPtr pData, IntPtr pIMalloc, uint size, uint codePage, out IDxcBlobEncoding ppBlobEncoding);

    [PreserveSig]
    int CreateBlob(IntPtr pData, uint size, uint codePage, out IDxcBlobEncoding ppBlobEncoding);

    /// <summary>Loads a file from disk into a blob.</summary>
    [PreserveSig]
    int LoadFile(string pFileName, in uint pCodePage,
        out IDxcBlobEncoding ppBlobEncoding);

    [PreserveSig]
    int CreateReadOnlyStreamFromBlob(IDxcBlob pBlob, out IntPtr ppStream);

    /// <summary>Creates a default include handler that reads from the file system.</summary>
    [PreserveSig]
    int CreateDefaultIncludeHandler(out IDxcIncludeHandler ppResult);

    [PreserveSig]
    int GetBlobAsUtf8(IDxcBlob pBlob, out IDxcBlobUtf8 ppBlobEncoding);

    [PreserveSig]
    int GetBlobAsWide(IDxcBlob pBlob, out IDxcBlobWide ppBlobEncoding);

    [PreserveSig]
    int GetDxilContainerPart(ref DxcBuffer pShader, uint dxcPart, out IntPtr ppPartData, out uint pPartSizeInBytes);

    [PreserveSig]
    int CreateReflection(ref DxcBuffer pData, in Guid riid, out IntPtr ppvReflection);
}

/// <summary>Main compiler interface (v3).</summary>
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper, StringMarshallingCustomType = typeof(WideStringMarshaller)), Guid("228B4687-5A6A-4730-900C-9702B2203F54")]
public partial interface IDxcCompiler3
{
    /// <summary>Compiles HLSL source to DXIL or other targets.</summary>
    /// <param name="pSource">Source buffer (encoding defined in the buffer).</param>
    /// <param name="pArguments">Array of compiler arguments (UTF‑16 on Windows, UTF‑32 elsewhere).</param>
    /// <param name="argCount">Number of arguments.</param>
    /// <param name="pIncludeHandler">Optional include handler (use default if null).</param>
    /// <param name="riid">Interface ID of the result (must be IID_IDxcResult).</param>
    /// <param name="ppResult">Receives the IDxcResult object.</param>
    [PreserveSig]
    int Compile(in DxcBuffer pSource,
        [In] string[] pArguments,
        uint argCount,
        IDxcIncludeHandler? pIncludeHandler,
        in Guid riid,
        out IntPtr ppResult);

    /// <summary>Disassembles a DXIL container or bitcode.</summary>
    [PreserveSig]
    int Disassemble(in DxcBuffer pObject,
        in Guid riid,
        out IntPtr ppResult);
}

// ====================================================================================
// DXC API entry points (P/Invoke)
// ====================================================================================

public static partial class DxcApi
{
    /// <summary>Creates a DXC component instance.</summary>
    [LibraryImport("dxcompiler")]
    private static partial int DxcCreateInstance(in Guid rclsid, in Guid riid, out IntPtr ppv);
    
    /// <summary>Creates a D3D DXC component instance.</summary>
    [LibraryImport("d3dcompiler", EntryPoint = nameof(DxcCreateInstance))]
    private static partial int D3DDxcCreateInstance(in Guid rclsid, in Guid riid, out IntPtr ppv);

    public static nint CreateDxcInstance(in Guid rclsid, in Guid riid)
    {
        DxcExceptionMarshaller.ThrowOnFailure(DxcCreateInstance(in rclsid, in riid, out var ptr));
        return ptr;
    }
    
    public static nint CreateD3DInstance(in Guid rclsid, in Guid riid)
    {
        DxcExceptionMarshaller.ThrowOnFailure(D3DDxcCreateInstance(in rclsid, in riid, out var ptr));
        return ptr;
    }
}

// ====================================================================================
// Exception handling and HRESULT mapping
// ====================================================================================

/// <summary>Base exception for DXC errors.</summary>
public class DxcException(string message, int hresult) : COMException(message, hresult);

/// <summary>Maps HRESULT values to specific DxcException-derived types.</summary>
public static class DxcExceptionMarshaller
{
    /// <summary>Throws an appropriate exception if the HRESULT indicates failure.</summary>
    public static void ThrowOnFailure(int hr)
    {
        if (hr >= 0) return;
        throw CreateException(hr);
    }
    
    /// <summary>Throws an appropriate exception if the HRESULT indicates failure.</summary>
    public static void ThrowOnFailure(int hr, string context)
    {
        if (hr >= 0) return;
        throw new COMException(context, CreateException(hr));
    }

    private static Exception CreateException(int hr)
    {
        // Map DXC-specific errors (facility 0xAA)
        if ((hr & 0xFFFF0000) == 0x80AA0000)
        {
            var code = hr & 0xFFFF;
            var message = code switch
            {
                0x0001 => "Overlapping semantics found.",
                0x0002 => "Multiple depth semantics found.",
                0x0003 => "Input file too large.",
                0x0004 => "Error parsing DXBC container.",
                0x0005 => "Error parsing DXBC bytecode.",
                0x0006 => "Data too large.",
                0x0007 => "Incompatible converter options.",
                0x0008 => "Irreducible control flow graph.",
                0x0009 => "IR verification error.",
                0x000A => "Scope-nested control flow recovery failed.",
                0x000B => "Operation not supported.",
                0x000C => "Unable to encode string.",
                0x000D => "DXIL container invalid.",
                0x000E => "DXIL container missing DXIL part.",
                0x000F => "Unable to parse DxilModule metadata.",
                0x0010 => "Error parsing DDI signature.",
                0x0011 => "Duplicate part in container.",
                0x0012 => "Missing part in container.",
                0x0013 => "Malformed container.",
                0x0014 => "Incorrect root signature.",
                0x0015 => "Container missing debug info.",
                0x0016 => "Macro expansion failure.",
                0x0017 => "DXIL optimization pass failed.",
                0x0018 => "General internal error.",
                0x0019 => "Abort compilation error.",
                0x001A => "Extension error.",
                0x001B => "LLVM fatal error.",
                0x001C => "LLVM unreachable.",
                0x001D => "LLVM cast error.",
                0x001E => "Validator missing.",
                0x001F => "Incorrect program version.",
                _ => $"Unknown DXC error (0x{hr:X8})"
            };
            return new DxcException(message, hr);
        }

        // Fallback to generic COM exception
        return Marshal.GetExceptionForHR(hr) ?? new DxcException($"HRESULT: 0x{hr:X8}", hr);
    }
}

public class D3DCompiler() : DxcCompiler(DxcApi.CreateD3DInstance(Guids.DxcCompiler, typeof(IDxcCompiler3).GUID));

// ====================================================================================
// High‑level wrapper classes (for easy consumption)
// ====================================================================================

/// <summary>Wrapper around IDxcCompiler3 that throws exceptions on failure.</summary>
public class DxcCompiler
{
    private readonly IDxcCompiler3 _compiler;

    /// <summary>Initializes a new instance of the DXC compiler.</summary>
    public DxcCompiler() : this(DxcApi.CreateDxcInstance(Guids.DxcCompiler, typeof(IDxcCompiler3).GUID))
    {
    }

    protected DxcCompiler(nint ptr)
    {
        unsafe
        {
            _compiler = ComInterfaceMarshaller<IDxcCompiler3>.ConvertToManaged((void*)ptr)!;
        }
    }

    /// <summary>Compiles HLSL source to DXIL or another target.</summary>
    /// <param name="source">Source buffer (text or binary).</param>
    /// <param name="arguments">Compiler arguments (e.g., "-E", "main", "-T", "ps_6_0").</param>
    /// <param name="includeHandler">Optional custom include handler; if null, a default file‑based handler is used.</param>
    /// <returns>An <see cref="IDxcResult"/> containing the compilation outputs.</returns>
    public IDxcResult Compile(DxcBuffer source, string[] arguments,
        IDxcIncludeHandler? includeHandler)
    {
        var hr = _compiler.Compile(in source, arguments, (uint)arguments.Length, includeHandler,
            typeof(IDxcResult).GUID, out var resultPtr);
        if (resultPtr == IntPtr.Zero)
            DxcExceptionMarshaller.ThrowOnFailure(hr);
        unsafe
        {
            return ComInterfaceMarshaller<IDxcResult>.ConvertToManaged((void*)resultPtr)!;
        }
    }
    
    /// <summary>Compiles HLSL source to DXIL or another target.</summary>
    /// <param name="source">Source text.</param>
    /// <param name="arguments">Compiler arguments (e.g., "-E", "main", "-T", "ps_6_0").</param>
    /// <param name="includeHandler">Optional custom include handler; if null, a default file‑based handler is used.</param>
    /// <returns>An <see cref="IDxcResult"/> containing the compilation outputs.</returns>
    public IDxcResult Compile(string source, string[] arguments,
        IDxcIncludeHandler? includeHandler)
    {
        unsafe
        {
            var buffer = new DxcBuffer
            {
                Encoding = DxcConstants.CP_UTF8,
                Ptr = (IntPtr)Utf8StringMarshaller.ConvertToUnmanaged(source),
                Size = (UIntPtr)Encoding.UTF8.GetByteCount(source) + 1
            };
            try
            {
                return Compile(buffer, arguments, includeHandler);
            }
            finally
            {
                Utf8StringMarshaller.Free((byte*)buffer.Ptr);
            }
        }
    }

    /// <summary>Preprocesses HLSL source (equivalent to Compile with -P).</summary>
    /// <param name="source">Source buffer.</param>
    /// <param name="arguments">Compiler arguments (should include "-P").</param>
    /// <param name="includeHandler">Optional include handler.</param>
    /// <returns>An <see cref="IDxcResult"/> whose primary output is the preprocessed text.</returns>
    public IDxcResult Preprocess(DxcBuffer source, string[] arguments,
        IDxcIncludeHandler? includeHandler)
    {
        // Add -P to arguments if not already present
        return Compile(source, !arguments.Contains("-P", StringComparer.Ordinal) ? [..arguments, "-P"] : arguments,
            includeHandler);
    }
    
    /// <summary>Preprocesses HLSL source (equivalent to Compile with -P).</summary>
    /// <param name="source">Source content.</param>
    /// <param name="arguments">Compiler arguments (should include "-P").</param>
    /// <param name="includeHandler">Optional include handler.</param>
    /// <returns>An <see cref="IDxcResult"/> whose primary output is the preprocessed text.</returns>
    public IDxcResult Preprocess(string source, string[] arguments,
        IDxcIncludeHandler? includeHandler)
    {
        // Add -P to arguments if not already present
        return Compile(source, !arguments.Contains("-P", StringComparer.Ordinal) ? [..arguments, "-P"] : arguments,
            includeHandler);
    }
}

/// <summary>Wrapper around IDxcUtils.</summary>
public class DxcUtils
{
    internal IDxcUtils Utils { get; }

    /// <summary>Initializes a new instance of the DXC utilities.</summary>
    public DxcUtils()
    {
        unsafe
        {
            Utils = ComInterfaceMarshaller<IDxcUtils>.ConvertToManaged(
                (void*)DxcApi.CreateDxcInstance(Guids.DxcUtils, typeof(IDxcUtils).GUID))!;
        }
    }

    /// <summary>Loads a file from disk into a blob.</summary>
    public IDxcBlobEncoding LoadFile(string fileName, uint? codePage = null)
    {
        var page = codePage.GetValueOrDefault();
        ref var pageRef = ref codePage.HasValue ? ref page : ref Unsafe.NullRef<uint>();
        var hr = Utils.LoadFile(fileName, in pageRef, out var blob);
        DxcExceptionMarshaller.ThrowOnFailure(hr);
        return blob;
    }

    /// <summary>Creates a blob from a managed byte array.</summary>
    public IDxcBlobEncoding CreateBlob(byte[] data, uint codePage = DxcConstants.CP_ACP)
    {
        var ptr = Marshal.AllocHGlobal(data.Length);
        try
        {
            Marshal.Copy(data, 0, ptr, data.Length);
            var hr = Utils.CreateBlob(ptr, (uint)data.Length, codePage, out var blob);
            DxcExceptionMarshaller.ThrowOnFailure(hr);
            return blob;
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }
    }

    /// <summary>Creates a blob from a string using the specified code page.</summary>
    public IDxcBlobEncoding CreateBlob(string text, uint codePage)
    {
        var bytes = codePage == DxcConstants.CP_UTF8 ? Encoding.UTF8.GetBytes(text) :
            codePage == DxcConstants.CP_UTF16 ? Encoding.Unicode.GetBytes(text) :
            codePage == DxcConstants.CP_UTF32 ? Encoding.UTF32.GetBytes(text) :
            Encoding.Default.GetBytes(text);
        return CreateBlob(bytes, codePage);
    }

    /// <summary>Creates a UTF‑8 blob from a string.</summary>
    public IDxcBlobUtf8 CreateUtf8Blob(string text)
    {
        var general = CreateBlob(text, DxcConstants.CP_UTF8);
        var hr = Utils.GetBlobAsUtf8(general, out var utf8Blob);
        DxcExceptionMarshaller.ThrowOnFailure(hr);
        return utf8Blob;
    }

    /// <summary>Creates a wide string blob (UTF‑16 on Windows, UTF‑32 elsewhere).</summary>
    public IDxcBlobWide CreateWideBlob(string text)
    {
        var general = CreateBlob(text, OperatingSystem.IsWindows() ? DxcConstants.CP_UTF16 : DxcConstants.CP_UTF32);
        var hr = Utils.GetBlobAsWide(general, out var wideBlob);
        DxcExceptionMarshaller.ThrowOnFailure(hr);
        return wideBlob;
    }
}

public static class DxcExtensions
{
    extension(IDxcBlob blob)
    {
        public string? GetString()
        {
            switch (blob)
            {
                case IDxcBlobWide blobWide:
                {
                    var size = blobWide.GetBufferSize() - (nuint)WideStringMarshaller.ElementSize;
                    var bufferPointer = blobWide.GetBufferPointer();
                    return size <= 0 || bufferPointer == 0 ? null : WideStringMarshaller.ConvertToManaged(bufferPointer);
                }
                case IDxcBlobUtf8 blobUtf8:
                {
                    var size = blobUtf8.GetBufferSize() - 1;
                    unsafe
                    {
                        var bufferPointer = blobUtf8.GetBufferPointer();
                        return size <= 0 || bufferPointer == 0 ? null : Encoding.UTF8.GetString((byte*)bufferPointer, (int)size);
                    }
                }
                case IDxcBlobEncoding blobEncoding:
                {
                    DxcExceptionMarshaller.ThrowOnFailure(blobEncoding.GetEncoding(out var known, out var codePage));
                    if (!known) return null;

                    var encoding = codePage switch
                    {
                        DxcConstants.CP_UTF8 => Encoding.UTF8,
                        DxcConstants.CP_UTF16 => Encoding.Unicode,
                        DxcConstants.CP_UTF32 => Encoding.UTF32,
                        _ => throw new ArgumentException($"Unsupported code page: {codePage}")
                    };

                    var size = blobEncoding.GetBufferSize() - (nuint)encoding.GetByteCount("\0");
                    unsafe
                    {
                        var bufferPointer = blobEncoding.GetBufferPointer();
                        return size <= 0 || bufferPointer == 0 ? null : encoding.GetString((byte*)bufferPointer, (int)size);
                    }
                }
                default:
                    return null;
            }
        }
    }
}

/// <summary>Helper class containing GUIDs for the DXC components.</summary>
internal static class Guids
{
    public static readonly Guid DxcCompiler =
        new(0x73e22d93, 0xe6ce, 0x47f3, 0xb5, 0xbf, 0xf0, 0x66, 0x4f, 0x39, 0xc1, 0xb0);

    public static readonly Guid DxcUtils = new(0x6245d6af, 0x66e0, 0x48fd, 0x80, 0xb4, 0x4d, 0x27, 0x17, 0x96, 0x74,
        0x8c);
}
