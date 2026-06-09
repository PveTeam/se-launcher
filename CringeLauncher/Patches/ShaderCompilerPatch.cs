using System.Diagnostics;
using System.Reflection.Emit;
using System.Runtime.InteropServices.Marshalling;
using CringeLauncher.Platform.Xplat;
using HarmonyLib;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using VRage.FileSystem;
using VRageRender;

#if !WINDOWS
namespace CringeLauncher.Patches;

[HarmonyPatchCategory("EarlyRender")]
[HarmonyPatch]
internal static partial class ShaderCompilerPatch
{
    private static readonly string BasePath = Path.Join(MyFileSystem.ShadersBasePath, "Shaders");

    private static readonly string[] BaseArguments =
    [
        "-flegacy-macro-expansion",
        "-flegacy-resource-reservation",
        "-fvk-use-dx-layout",
        "-HV", "2016",
        "-D", "HLSL_VERSION=2016",
        "-D", "FLOAT=float",
        "-I", BasePath
    ];

    private static readonly Lazy<DxcCompiler> Dxc = new();
    private static readonly Lazy<D3DCompiler> D3D = new();
    private static readonly Lazy<DxcIncludeHandler> DxcInclude = new();

    [HarmonyPatch(typeof(MyShaderCompiler), nameof(MyShaderCompiler.PreprocessShader))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> PreprocessTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var originalPreprocess = AccessTools.DeclaredMethod(typeof(ShaderBytecode),
            nameof(ShaderBytecode.PreprocessFromFile),
            [typeof(string), typeof(ShaderMacro[]), typeof(Include), typeof(string).MakeByRefType()]);
        var preprocess = AccessTools.DeclaredMethod(typeof(ShaderCompilerPatch), nameof(PreprocessFromFile));
        return instructions.MethodReplacer(originalPreprocess, preprocess);
    }

    [HarmonyPatch(typeof(MyShaderCompiler), nameof(MyShaderCompiler.Compile),
    [
        typeof(string), typeof(ShaderMacro[]), typeof(MyShaderProfile),
        typeof(string), typeof(bool), typeof(bool),
        typeof(bool), typeof(string), typeof(string),
        typeof(bool), typeof(bool)
    ], [
        ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
        ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal,
        ArgumentType.Out, ArgumentType.Out, ArgumentType.Out,
        ArgumentType.Normal, ArgumentType.Normal
    ])]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CompileTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var originalCompile = AccessTools.DeclaredMethod(typeof(ShaderBytecode),
            nameof(ShaderBytecode.Compile),
            [
                typeof(string), typeof(string), typeof(string), typeof(ShaderFlags), typeof(EffectFlags),
                typeof(ShaderMacro[]), typeof(Include), typeof(string), typeof(SecondaryDataFlags), typeof(DataStream)
            ]);
        var compile = AccessTools.DeclaredMethod(typeof(ShaderCompilerPatch), nameof(Compile));
        var originalStrip = AccessTools.DeclaredMethod(typeof(ShaderBytecode), nameof(ShaderBytecode.Strip));
        return new CodeMatcher(instructions)
            .MatchStartForward(CodeMatch.Calls(originalCompile))
            .Set(OpCodes.Call, compile)
            .Start()
            .MatchStartForward(CodeMatch.Calls(originalStrip))
            .SetInstruction(CodeInstruction.CallClosure((ShaderBytecode b, StripFlags flags) => b))
            .InstructionEnumeration();
    }

    private static string? PreprocessFromFile(
        string fileName,
        ShaderMacro[] defines,
        Include include,
        out string? compilationErrors)
    {
        try
        {
            LauncherFileProvider.Instance.NormalizePath(ref fileName);
            var source = File.ReadAllText(fileName);
            compilationErrors = null;
            var result = Dxc.Value.Preprocess(source,
            [
                ..BaseArguments, "-P",
                "-I", Path.GetDirectoryName(fileName)!,
                ..SerializeDefines(defines),
                fileName
            ], DxcInclude.Value);

            DxcExceptionMarshaller.ThrowOnFailure(result.GetStatus(out var status));

            if (result.HasOutput(DxcOutputKind.Errors))
            {
                DxcExceptionMarshaller.ThrowOnFailure(result.GetOutput(DxcOutputKind.Errors, typeof(IDxcBlob).GUID,
                    out var errorBufferPtr, out _));
                unsafe
                {
                    var errorBuffer = ComInterfaceMarshaller<IDxcBlob>.ConvertToManaged((void*)errorBufferPtr)!;
                    compilationErrors = errorBuffer.GetString();
                }
            }

            if (status != 0) return null;

            if (result.HasOutput(DxcOutputKind.Hlsl))
            {
                DxcExceptionMarshaller.ThrowOnFailure(result.GetOutput(DxcOutputKind.Hlsl, typeof(IDxcBlob).GUID,
                    out var bufferPtr, out _));
                unsafe
                {
                    var buffer = ComInterfaceMarshaller<IDxcBlob>.ConvertToManaged((void*)bufferPtr)!;
                    var str = buffer.GetString();
                    if (str is not null) return str;
                }
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private static IEnumerable<string> SerializeDefines(ShaderMacro[] defines) =>
        defines.SelectMany(b => new[]
        {
            "-D",
            string.IsNullOrEmpty(b.Definition)
                ? b.Name
                : $"{b.Name}={string.Join(" ", b.Definition.ReplaceLineEndings().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Select(s => s.TrimEnd('\\')))}"
        });

    private static CompilationResult Compile(
        string shaderSource,
        string entryPoint,
        string profile,
        ShaderFlags shaderFlags,
        EffectFlags effectFlags,
        ShaderMacro[] defines,
        Include include,
        string sourceFileName = "unknown",
        SecondaryDataFlags secondaryDataFlags = SecondaryDataFlags.None,
        DataStream? secondaryData = null)
    {
        LauncherFileProvider.Instance.NormalizePath(ref sourceFileName);
        var (_, profileMajor, _) = CompileProfile.Parse(profile);
        var compiler = profileMajor > 5 ? Dxc.Value : D3D.Value;
        var result = compiler.Compile(shaderSource,
        [
            "-spirv",
            "-fspv-emit-binding-info",
            "-fspv-dxvk-layout",
            "-fspv-reflect",
            ..BaseArguments,
            ..SerializeDefines(defines),
            "-E", entryPoint,
            "-T", profile,
            sourceFileName
        ], DxcInclude.Value);

        DxcExceptionMarshaller.ThrowOnFailure(result.GetStatus(out var status));

        string? compilationErrors = null;
        if (result.HasOutput(DxcOutputKind.Errors))
        {
            DxcExceptionMarshaller.ThrowOnFailure(result.GetOutput(DxcOutputKind.Errors, typeof(IDxcBlob).GUID,
                out var errorBufferPtr, out _));
            unsafe
            {
                var errorBuffer = ComInterfaceMarshaller<IDxcBlob>.ConvertToManaged((void*)errorBufferPtr)!;
                compilationErrors = errorBuffer.GetString();
            }
        }

        ShaderBytecode? shaderBytecode = null;
        if (status == 0 && result.HasOutput(DxcOutputKind.Object))
        {
            unsafe
            {
                DxcExceptionMarshaller.ThrowOnFailure(result.GetOutput(DxcOutputKind.Object, typeof(IDxcBlob).GUID,
                    out var bufferPtr, out _));
                var resultBuffer = ComInterfaceMarshaller<IDxcBlob>.ConvertToManaged((void*)bufferPtr)!;
                shaderBytecode = new ShaderBytecode(resultBuffer.GetBufferPointer(),
                    (int)resultBuffer.GetBufferSize());

                // Dump SPIR-V binary for debugging: filename contains profile, entrypoint,
                // SPIR-V MD5 and source MD5 so it can be correlated with DXVK runtime dumps.
                try
                {
                    var spirvBytes = new ReadOnlySpan<byte>((void*)resultBuffer.GetBufferPointer(),
                        (int)resultBuffer.GetBufferSize());
                    var hashData = System.Security.Cryptography.MD5.HashData(
                        spirvBytes);
                    var spirvMd5Hash = Convert.ToHexStringLower(hashData);

                    // Compute DXVK-style ShaderKey (XOR-folded MD5, same as DxvkShaderHash)
                    uint[] dxvkHash = new uint[4];
                    for (int i = 0; i < 16; i += 4)
                        dxvkHash[i / 4] = BitConverter.ToUInt32(hashData, i);
                    var dxvkKey = string.Concat(
                        profile.StartsWith("cs") ? "cs" :
                        profile.StartsWith("vs") ? "vs" :
                        profile.StartsWith("ps") ? "fs" :
                        profile.StartsWith("gs") ? "gs" : "shdr",
                        ".",
                        dxvkHash[0].ToString("x8"), dxvkHash[1].ToString("x8"),
                        dxvkHash[2].ToString("x8"), dxvkHash[3].ToString("x8"));

                    var profileStr = profile.Replace('_', '-');
                    var dumpPath = $"/tmp/dxc_spv_{profileStr}_{entryPoint}_{dxvkKey[3..]}_{spirvMd5Hash[..16]}.spv";
                    File.WriteAllBytes(dumpPath, spirvBytes);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Failed to dump shader {sourceFileName}: {e}");
                    // Best-effort dump, never fail compilation over it
                }
            }
        }

        return new(shaderBytecode, status, compilationErrors);
    }

    private record struct CompileProfile(string Type, int Major, int Minor)
    {
        public static CompileProfile Parse(string profile)
        {
            var type = profile[..2];
            var major = int.Parse(profile[3..4]);
            var minor = int.Parse(profile[5..6]);
            return new(type, major, minor);
        }
    }

    [GeneratedComClass]
    private sealed partial class DxcIncludeHandler : IDxcIncludeHandler
    {
        private readonly DxcUtils _utils = new();

        public IDxcBlob? LoadSource(string pFilename)
        {
            pFilename = Path.GetFullPath(pFilename);
            LauncherFileProvider.Instance.NormalizePath(ref pFilename);
            if (File.Exists(pFilename))
                return _utils.LoadFile(pFilename, DxcConstants.CP_UTF8);

            Debug.WriteLine($"Cant open {pFilename}");
            return null;
        }
    }
}
#endif
