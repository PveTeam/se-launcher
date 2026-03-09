using System.Runtime.InteropServices;
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
        var strip = AccessTools.DeclaredMethod(typeof(ShaderCompilerInterface),
            nameof(ShaderCompilerInterface.Strip));
        return instructions.MethodReplacer(originalCompile, compile).MethodReplacer(originalStrip, strip);
    }

    private static string PreprocessFromFile(
        string fileName,
        ShaderMacro[] defines,
        Include include,
        out string? compilationErrors)
    {
        LauncherFileProvider.Instance.NormalizePath(ref fileName);
        var source = File.ReadAllText(fileName);
        compilationErrors = null;
        return ShaderCompilerInterface.Preprocess(source, fileName, defines,
            new IncludeProvider(Path.GetDirectoryName(fileName)!));
    }

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
        return ShaderCompilerInterface.Compile(shaderSource, entryPoint, profile, shaderFlags, effectFlags, defines,
            new IncludeProvider(Path.GetDirectoryName(sourceFileName)!), sourceFileName);
    }

    [GeneratedComClass]
    private sealed unsafe partial class IncludeProvider(string sourceDir) : IDxcIncludeProvider 
    {
        private readonly string _basePath = Path.Join(MyFileSystem.ShadersBasePath, "Shaders");

        private readonly Dictionary<nint, string> _sourceDirs = [];

        public int Open(IncludeType includeType, string fileName, nint pParentData, out nint pData)
        {
            try
            {
                var path = Path.Join(includeType == IncludeType.Local ? pParentData == 0 ? sourceDir : _sourceDirs[pParentData] : _basePath,
                    fileName);
                path = Path.GetFullPath(path);
                using var stream =
                    LauncherFileProvider.Instance.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (stream is null) throw new FileNotFoundException($"Cant open {includeType} {fileName} header", path);
                var ptr = (byte*)NativeMemory.Alloc((nuint)stream.Length);
                pData = (nint)ptr;
                _sourceDirs[pData] = Path.GetDirectoryName(path)!;
                var buffer = new Span<byte>(ptr, (int)stream.Length);
                stream.ReadExactly(buffer);
                return buffer.Length;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void Close(nint pData)
        {
            NativeMemory.Free((void*)pData);
        }
    }
}
#endif
