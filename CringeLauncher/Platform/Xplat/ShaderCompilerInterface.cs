#if !WINDOWS
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using CringeLauncher.Utils;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using Silk.NET.Core.Native;

namespace CringeLauncher.Platform.Xplat;

internal static unsafe partial class ShaderCompilerInterface
{
    public static string Preprocess(string shaderSource, string sourceName, ShaderMacro[] macros, IDxcIncludeProvider? includeProvider)
    {
        var pDefines = stackalloc D3DShaderMacro[macros.Length + 1];
        for (var i = 0; i < macros.Length; i++)
        {
            ref var pMacro = ref pDefines[i];
            var macro = macros[i];
            
            pMacro.Name = Utf8StringMarshaller.ConvertToUnmanaged(macro.Name);
            pMacro.Definition = Utf8StringMarshaller.ConvertToUnmanaged(macro.Definition);
        }

        var hr = (Result)Preprocess(shaderSource, (nuint)Encoding.UTF8.GetByteCount(shaderSource), sourceName, pDefines,
            includeProvider, out var pResult, out var pErrors);

        for (var i = 0; i < macros.Length; i++)
        {
            ref var pMacro = ref pDefines[i];
            
            Utf8StringMarshaller.Free(pMacro.Name);
            Utf8StringMarshaller.Free(pMacro.Definition);
        }

        if (hr.Success) 
            return Marshal.PtrToStringAnsi(pResult.GetBufferPointer(), (int)pResult.GetBufferSize());
        
        if (pErrors is not null)
        {
            var compilationException = new CompilationException(hr,
                Marshal.PtrToStringAnsi(pErrors.GetBufferPointer(), (int)pErrors.GetBufferSize()));
            Console.Error.WriteLine(compilationException);
            throw compilationException;
        }

        throw new SharpDXException(hr);
    }
    
    public static ShaderBytecode Strip(ShaderBytecode bytecode, StripFlags flags)
    {
        fixed (byte* ptr = &bytecode.Data[0])
        {
            var hr = (Result)StripShader((nint)ptr, (nuint)bytecode.Data.Length, flags, out var pStrippedBytecode);
            
            hr.CheckError();

            return new(pStrippedBytecode.GetBufferPointer(), (int)pStrippedBytecode.GetBufferSize());
        }
    }

    public static CompilationResult Compile(string shaderSource,
        string entryPoint,
        string profile,
        ShaderFlags shaderFlags,
        EffectFlags effectFlags,
        ShaderMacro[] macros,
        IDxcIncludeProvider? includeProvider,
        string sourceName)
    {
        var pDefines = stackalloc D3DShaderMacro[macros.Length + 1];
        for (var i = 0; i < macros.Length; i++)
        {
            ref var pMacro = ref pDefines[i];
            var macro = macros[i];
            
            pMacro.Name = Utf8StringMarshaller.ConvertToUnmanaged(macro.Name);
            pMacro.Definition = Utf8StringMarshaller.ConvertToUnmanaged(macro.Definition);
        }

        var hr = (Result)Compile(shaderSource, (nuint)Encoding.UTF8.GetByteCount(shaderSource), sourceName, pDefines,
            includeProvider, entryPoint, profile, shaderFlags, effectFlags, 0, 0, 0, out var pResult, out var pErrors);

        for (var i = 0; i < macros.Length; i++)
        {
            ref var pMacro = ref pDefines[i];
            
            Utf8StringMarshaller.Free(pMacro.Name);
            Utf8StringMarshaller.Free(pMacro.Definition);
        }

        if (hr.Success) 
            return new(new(pResult.GetBufferPointer(), (int)pResult.GetBufferSize()), hr, string.Empty);
        
        if (pErrors is not null)
        {
            var compilationException = new CompilationException(hr,
                Marshal.PtrToStringAnsi(pErrors.GetBufferPointer(), (int)pErrors.GetBufferSize()));
            Console.Error.WriteLine(compilationException);
            throw compilationException;
        }

        throw new SharpDXException(hr);
    }
    
    [LibraryImport(PlatformApi.PlatformDllName, EntryPoint = $"{PlatformApi.CallPrefix}D3DPreprocess",
        StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Preprocess(string shaderSource, nuint shaderSourceLength, string sourceName,
        D3DShaderMacro* pDefines, IDxcIncludeProvider? pInclude, out IDxcBlob pResult, out IDxcBlob? pErrors);
    
    [LibraryImport(PlatformApi.PlatformDllName, EntryPoint = $"{PlatformApi.CallPrefix}D3DStripShader",
        StringMarshalling = StringMarshalling.Utf8)]
    private static partial int StripShader(nint shaderBytecode, nuint shaderBytecodeLength, StripFlags flags, out IDxcBlob pStrippedBytecode);

    [LibraryImport(PlatformApi.PlatformDllName, EntryPoint = $"{PlatformApi.CallPrefix}D3DCompile",
        StringMarshalling = StringMarshalling.Utf8)]
    private static partial int Compile(string shaderSource, nuint shaderSourceLength, string sourceName,
        D3DShaderMacro* pDefines, IDxcIncludeProvider? pInclude, string entrypoint, string target,
        ShaderFlags shaderFlags, EffectFlags effectFlags, uint secondaryDataFlags, nint secondaryData,
        nuint secondaryDataSize, out IDxcBlob pResult, out IDxcBlob? pErrors);
}

[Guid("AB205C7C-C3B8-4D75-93C7-8FB919A4A9AF")]
[GeneratedComInterface(Options = ComInterfaceOptions.ManagedObjectWrapper, StringMarshalling = StringMarshalling.Utf8)]
internal partial interface IDxcIncludeProvider
{
    /// <summary>
    /// A user-implemented method for opening and reading the contents of a shader
    /// </summary>
    /// <param name="includeType">Value that indicates the location of the #include file.</param>
    /// <param name="fileName">Name of the #include file.</param>
    /// <param name="pData">Pointer to the buffer that contains the include directives. This pointer remains valid until you call <see cref="Close"/></param>
    /// <returns>Number of bytes that Open returns in <see cref="pData"/>.</returns>
    [SuppressMessage("ComInterfaceGenerator",
        "SYSLIB1092:The return value in the managed definition will be converted to an additional \'out\' parameter at the end of the parameter list when calling the unmanaged COM method.",
        Justification = "Return is cBytes")]
    int Open(IncludeType includeType, string fileName, nint pParentData, out nint pData);

    /// <summary>
    /// A user-implemented method for closing a shader #include file.
    /// </summary>
    /// <param name="pData">Pointer to the buffer that contains the include directives. This is the pointer that was returned by the corresponding <see cref="Open"/> call.</param>
    void Close(nint pData);
}

[Guid("6101C721-C6A7-4E37-9A04-91C64D38C396")]
[GeneratedComInterface(Options = ComInterfaceOptions.ComObjectWrapper)]
internal partial interface IDxcBlob
{
    [PreserveSig]
    nint GetBufferPointer();
    
    [PreserveSig]
    nuint GetBufferSize();
}
#endif
