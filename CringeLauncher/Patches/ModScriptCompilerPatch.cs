using CringeBootstrap.Abstractions;
using CringeLauncher.CrashPad;
using CringeLauncher.Loader;
using CringeLauncher.SyntaxRewriters;
using CringePlugins.Config;
using CringePlugins.Services;
using HarmonyLib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using Sandbox;
using Sandbox.Game;
using Sandbox.Game.Entities.Blocks;
using Sandbox.Game.EntityComponents;
using Sandbox.Game.GameSystems.TextSurfaceScripts;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Ingame;
using Steamworks;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.ModAPI;
using VRage.Scripting;
using Message = VRage.Scripting.Message;

namespace CringeLauncher.Patches;

[HarmonyPatch]
public static class ModScriptCompilerPatch
{
    public static readonly ConcurrentDictionary<Assembly, string> AssemblyCacheLookup = [];
    internal static readonly MyConcurrentHashSet<MyProgrammableBlock> CompilingPbs = [];

    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private static ModAssemblyLoadContext _modContext;
    private static readonly MyConcurrentHashSet<string> LoadedModAssemblyNames = [];

    private static readonly ConditionalWeakTable<MyProgrammableBlock, PbAssemblyLoadContext> LoadContexts = [];

    private static readonly FieldInfo InstanceField = AccessTools.Field(typeof(MyProgrammableBlock), "m_instance");
    private static readonly PropertyInfo AssemblyProperty = AccessTools.Property(typeof(MyProgrammableBlock), "CurrentAssembly");
    private static readonly FieldInfo CompilerErrorsField = AccessTools.Field(typeof(MyProgrammableBlock), "m_compilerErrors");
    private static readonly MethodInfo CreateInstanceMethod = AccessTools.Method(typeof(MyProgrammableBlock), "CreateInstance");
    private static readonly MethodInfo SetDetailedInfoMethod = AccessTools.Method(typeof(MyProgrammableBlock), "SetDetailedInfo");
    private static readonly ICoreLoadContext CoreContext = (ICoreLoadContext)AssemblyLoadContext.GetLoadContext(typeof(MySession).Assembly)!;

    private static readonly ConfigReference<LauncherConfig> LauncherConfigRef =
        MySandboxGame.Services.GetRequiredService<ConfigHandler>().RegisterConfig("launcher", LauncherConfig.Default);

    private static readonly string CrossGenCacheKey = MySandboxGame.Services.GetRequiredService<ICrossGenService>()
        .CacheKey;

    private static readonly CrashPadService CrashPad = MySandboxGame.Services.GetRequiredService<CrashPadService>();

    static ModScriptCompilerPatch()
    {
        _modContext = new(CoreContext);
    }

    [HarmonyPrepare]
    private static void Prepare(MethodBase? original)
    {
        if (original is not null)
            return;

        MySession.OnUnloaded += OnUnloaded;

        MyScriptManager.m_compatibilityChanges.Remove("using VRage.Common.Voxels;");
        MyScriptManager.m_compatibilityChanges.Remove("using Sandbox.Common.ObjectBuilders.Serializer;");
        MyScriptManager.m_compatibilityChanges.Remove("using Sandbox.Common.ObjectBuilders.VRageData;");
        MyScriptManager.m_compatibilityChanges.Remove("using Sandbox.Common.Input;");
        MyScriptManager.m_compatibilityChanges.Remove("using Sandbox.Common.ModAPI;");
        MyScriptManager.m_compatibilityChanges.Add("FirstOrDefault(null)", "FirstOrDefault()");
        MyScriptManager.m_compatibilityChanges.Add("using System.Numerics;", "using VRageMath;"); //todo: investigate

        MyModWatchdog.ModInfo = [new("Unknown")];

        var modDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "CringeLauncher", "cache", "mods");
        var scriptDir = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "CringeLauncher", "cache", "scripts");

        if (LauncherConfigRef.Value.CacheModAssemblies && !Directory.Exists(Path.Join(modDir, CrossGenCacheKey)))
        {
            if (Directory.Exists(modDir))
                ClearOldCache(modDir);

            Directory.CreateDirectory(Path.Join(modDir, CrossGenCacheKey));
        }

        if (LauncherConfigRef.Value.CacheScriptAssemblies && !Directory.Exists(Path.Join(scriptDir, CrossGenCacheKey)))
        {
            if (Directory.Exists(scriptDir))
                ClearOldCache(scriptDir);

            Directory.CreateDirectory(Path.Join(scriptDir, CrossGenCacheKey));
        }
    }

    private static void OnUnloaded()
    {
        LoadedModAssemblyNames.Clear();
        AssemblyCacheLookup.Clear();
        CrashPad.ClearModScripts();
        CrashPad.MarkSavePoint();

        if (!_modContext.Assemblies.Any())
            return;

        _modContext.Unload();
        _modContext = new(CoreContext);
    }

    [HarmonyPatch(typeof(MyProgrammableBlock), "Compile")]
    [HarmonyPrefix]
    private static bool CompilePrefix(MyProgrammableBlock __instance, string program, string storage, bool instantiate,
                                      ref MyProgrammableBlock.ScriptTerminationReason ___m_terminationReason,
                                      MyIngameScriptComponent ___m_scriptComponent)
    {
        if (!MySession.Static.EnableIngameScripts || __instance.CubeGrid is { IsPreview: true } or { CreatePhysics: false } || !CompilingPbs.Add(__instance))
            return false;

        ___m_terminationReason = MyProgrammableBlock.ScriptTerminationReason.None;
        CompileAsync(__instance, program, storage, instantiate, ___m_scriptComponent);
        return false;
    }

    [HarmonyPatch(typeof(MyGuiScreenEditor), "CheckCodeButtonClicked")]
    [HarmonyPrefix]
    private static bool GuiCompilePrefix(List<string> ___m_compilerErrors, MyGuiScreenEditor __instance)
    {
        ___m_compilerErrors.Clear();

        var progress = new MyGuiScreenProgress(MyTexts.Get(MySpaceTexts.ProgrammableBlock_Editor_CheckingCode));
        MyScreenManager.AddScreen(progress);

        if (__instance.Description.Text.Length > 0)
        {
            var task = CompileAsync(__instance, ___m_compilerErrors, __instance.Description.Text.ToString(), progress);
            task.ConfigureAwait(false).GetAwaiter().GetResult();

            MyScreenManager.RemoveScreen(progress);

            MyVRage.Platform.ImeProcessor?.RegisterActiveScreen(__instance);
            __instance.FocusedControl = __instance.Description;
        }

        return false;
    }

    [HarmonyPatch(typeof(MyScriptCompiler), nameof(MyScriptCompiler.Compile))]
    [HarmonyPrefix]
    private static bool Prefix(ref Task<Assembly?> __result, MyApiTarget target, string assemblyName, IEnumerable<Script> scripts,
        List<Message> messages, string friendlyName, bool enableDebugInformation = false)
    {
        __result = CompileAsync(_modContext, target, assemblyName, scripts, messages, friendlyName,
            enableDebugInformation);
        return false;
    }

    [HarmonyPatch(typeof(MyTextSurfaceScriptFactory), nameof(MyTextSurfaceScriptFactory.LoadScripts))]
    [HarmonyPrefix]
    private static void FinishLoadingScripts() => CrashPad.MarkSavePoint();

    private static async Task CompileAsync(MyGuiScreenEditor editor, List<string> errors, string program, MyGuiScreenProgress progress)
    {
        var context = new PbAssemblyLoadContext(CoreContext, editor.Name);
        var messages = new List<Message>();
        var script = MyVRage.Platform.Scripting.GetIngameScript(program, "Program", nameof(MyGridProgram));
        await CompileAsync(context, MyApiTarget.Ingame, "check", [script], messages,
            "PB Code Editor", true);

        errors.AddRange(messages.OrderBy(b => b.IsError ? 0 : 1).Select(b => b.Text));
        context.Unload();

        progress.CloseScreen();

        if (errors.Count > 0)
        {
            var sb = new StringBuilder(errors.Sum(b => b.Length + Environment.NewLine.Length));
            foreach (var error in errors)
            {
                sb.AppendLine(error);
            }

            MyScreenManager.AddScreen(new MyGuiScreenEditorError(sb.ToString()));
            return;
        }

        var messageBox = MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.OK,
                                                       MyTexts.Get(MySpaceTexts.ProgrammableBlock_Editor_CompilationOk),
                                                       MyTexts.Get(MySpaceTexts.ProgrammableBlock_CodeEditor_Title));
        MyGuiSandbox.AddScreen(messageBox);
    }

    private static async void CompileAsync(MyProgrammableBlock block,
                                           string program,
                                           string storage,
                                           bool instantiate, MyIngameScriptComponent scriptComponent)
    {
        try
        {
            scriptComponent.NeedsUpdate = MyEntityUpdateEnum.NONE;
            scriptComponent.UpdateFrequency = UpdateFrequency.None;

            SetDetailedInfoMethod.Invoke(block, ["Compiling..."]);

            if (LoadContexts.TryGetValue(block, out var context))
            {
                AccessTools.FieldRefAccess<MyProgrammableBlock, IMyGridProgram?>(block, InstanceField) = null;
                AssemblyProperty.SetValue(block, null);
                context.Unload();
            }

            LoadContexts.AddOrUpdate(block, context = new(CoreContext, $"pb_{block.EntityId}"));

            var messages = new List<Message>();
            var assembly = await CompileAsync(context, MyApiTarget.Ingame, $"pb_{block.EntityId}_{Random.Shared.NextInt64()}",
                                              [MyVRage.Platform.Scripting.GetIngameScript(program, "Program", nameof(MyGridProgram))],
                                              messages, $"PB: {block.DisplayName} ({block.EntityId})", true);

            AssemblyProperty.SetValue(block, assembly);

            var errors = AccessTools.FieldRefAccess<MyProgrammableBlock, List<string>>(block, CompilerErrorsField);

            errors.Clear();
            errors.AddRange(messages.Select(b => b.Text));

            if (instantiate)
            {
                MySandboxGame.Static.Invoke(() => CreateInstanceMethod.Invoke(block, [assembly, errors, storage]),
                    nameof(CompileAsync));
            }
        }
        catch (Exception e)
        {
            SetDetailedInfoMethod.Invoke(block, [e.ToString()]);
            Log.Error(e);
        }
        finally
        {
            CompilingPbs.Remove(block);
        }
    }

    private static async Task<Assembly?> CompileAsync(AssemblyLoadContext context, MyApiTarget target,
                                                      string assemblyName, IEnumerable<Script> scripts,
                                                      List<Message> messages, string? friendlyName, bool trackMemoryUsage = false,
                                                      bool enableDebugInformation = false)
    {
        friendlyName ??= "<No Name>";
        var assemblyFileName = MyScriptCompiler.MakeAssemblyName(assemblyName);
        Func<CSharpCompilation, SyntaxTree, bool, SyntaxTree>? syntaxTreeInjector;
        DiagnosticAnalyzer? whitelistAnalyzer;

        Debug.WriteLine(assemblyName);
        Debug.WriteLine(assemblyFileName);

        string? cachePath = null;

        switch (target)
        {
            case MyApiTarget.None:
                whitelistAnalyzer = null;
                syntaxTreeInjector = null;
                break;
            case MyApiTarget.Mod:
                {
                    //skip if name exists already
                    if (!LoadedModAssemblyNames.Add(assemblyFileName))
                    {
                        Console.WriteLine($"{assemblyFileName} is already loaded, skipping");
                        return null;
                    }


                    var ind = assemblyFileName.IndexOf('.');
                    var idStr = ind > 0 ? assemblyFileName[..ind] : "";
                    if (LauncherConfigRef.Value.CacheModAssemblies && ulong.TryParse(idStr, out var id) && SteamUGC.GetItemInstallInfo((PublishedFileId_t)id, out _, out _, 260U, out var timestamp))
                    {
                        cachePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "CringeLauncher", "cache", "mods", CrossGenCacheKey, $"{assemblyFileName}-{timestamp}.cache");

                        if (File.Exists(cachePath))
                        {
                            for (var i = 0; i < 200; i++)
                            {
                                try
                                {
                                    await using var ms = new MemoryStream(await File.ReadAllBytesAsync(cachePath));
                                    var assembly = context.LoadFromStream(ms);

                                    AssemblyCacheLookup[assembly] = cachePath;

                                    CrashPad.RegisterModScript(assemblyFileName, true);

                                    return assembly;
                                }
                                catch (IOException) //retry if file is in use
                                {
                                    await Task.Delay(5);
                                }
                            }
                        }
                    }

                    whitelistAnalyzer = MyScriptCompiler.Static.m_modApiWhitelistDiagnosticAnalyzer;
                    syntaxTreeInjector = MissingUsingRewriter.Rewrite;

                    scripts = await Task.WhenAll(scripts.Select(LoadModScript));
                    break;
                }
            case MyApiTarget.Ingame:

                if (LauncherConfigRef.Value.CacheScriptAssemblies)
                {
                    await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(scripts.First().Code));
                    var bytes = await MD5.HashDataAsync(stream);
                    var hash = Convert.ToHexString(bytes);

                    cachePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                "CringeLauncher", "cache", "scripts", CrossGenCacheKey, $"{hash}.cache");

                    if (File.Exists(cachePath))
                    {
                        for (var i = 0; i < 200; i++)
                        {
                            try
                            {
                                await using var ms = new MemoryStream(await File.ReadAllBytesAsync(cachePath));
                                var assembly = context.LoadFromStream(ms);

                                AssemblyCacheLookup[assembly] = cachePath;

                                return assembly;
                            }
                            catch (IOException) //retry if file is in use
                            {
                                await Task.Delay(5);
                            }
                        }
                    }
                }

                syntaxTreeInjector = MyScriptCompiler.Static.InjectResourceMonitoring;
                whitelistAnalyzer = MyScriptCompiler.Static.m_inGameWhitelistDiagnosticAnalyzer;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, "Invalid compilation target");
        }
        var compilation = MyScriptCompiler.Static.CreateCompilation(assemblyFileName, scripts, enableDebugInformation);
        var compilationWithoutInjection = compilation;
        var injectionFailed = false;

        if (syntaxTreeInjector != null)
        {
            SyntaxTree[]? newSyntaxTrees = null;
            try
            {
                var syntaxTrees = compilation.SyntaxTrees;
                if (syntaxTrees.Length == 1)
                {
                    newSyntaxTrees = [syntaxTreeInjector(compilation, syntaxTrees[0], trackMemoryUsage)];
                }
                else
                {
                    var compilation1 = compilation;
                    newSyntaxTrees = await Task
                        .WhenAll(syntaxTrees.Select(
                            x => Task.Run(() => syntaxTreeInjector(compilation1, x, trackMemoryUsage)))).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
                injectionFailed = true;

                if (target == MyApiTarget.Mod)
                    CrashPad.RegisterModScript(assemblyFileName, false, e.ToString());
            }

            if (newSyntaxTrees is not null)
                compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(newSyntaxTrees);
        }
        CompilationWithAnalyzers? analyticCompilation = null;
        if (whitelistAnalyzer != null)
        {
            analyticCompilation = compilation.WithAnalyzers([whitelistAnalyzer]);
            compilation = (CSharpCompilation)analyticCompilation.Compilation;
        }

        await using var assemblyStream = new MemoryStream();

        var emitResult = compilation.Emit(assemblyStream);
        var success = emitResult.Success;
        var myBlacklistSyntaxVisitor = new MyBlacklistSyntaxVisitor();
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            myBlacklistSyntaxVisitor.SetSemanticModel(compilation.GetSemanticModel(syntaxTree, false));
            myBlacklistSyntaxVisitor.Visit(await syntaxTree.GetRootAsync());
        }
        if (myBlacklistSyntaxVisitor.HasAnyResult())
        {
            myBlacklistSyntaxVisitor.GetResultMessages(messages);
        }
        else
        {
            success = await MyScriptCompiler.Static.EmitDiagnostics(analyticCompilation!, emitResult, messages, success).ConfigureAwait(false);
            assemblyStream.Seek(0, SeekOrigin.Begin);
            if (injectionFailed)
                return null;
            if (success)
            {
                if (cachePath is not null)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(cachePath)!);
                    await using var fileStream = File.Create(cachePath);
                    await assemblyStream.CopyToAsync(fileStream);

                    assemblyStream.Seek(0, SeekOrigin.Begin);
                }
                var assembly = context.LoadFromStream(assemblyStream);

                if (cachePath is not null)
                    AssemblyCacheLookup[assembly] = cachePath;

                if (target == MyApiTarget.Mod)
                    CrashPad.RegisterModScript(assemblyFileName, false);

                return assembly;
            }

            await MyScriptCompiler.Static.EmitDiagnostics(analyticCompilation!, compilationWithoutInjection.Emit(assemblyStream), messages,
                false).ConfigureAwait(false);
        }

        if (target == MyApiTarget.Mod)
            CrashPad.RegisterModScript(assemblyFileName, false, string.Join("\n", messages.Where(b => b.IsError).Select(b => b.Text)));

        return null;
    }

    private static async Task<Script> LoadModScript(Script script)
    {
        var text = await File.ReadAllTextAsync(script.Code);

        foreach ((var old, var @new) in MyScriptManager.m_compatibilityChanges)
        {
            text = text.Replace(old, @new, StringComparison.Ordinal);
        }

        return new(script.Name, text.Insert(0, MyScriptManager.COMPATIBILITY_USINGS));
    }

    private static void ClearOldCache(string dir)
    {
        foreach (var directory in Directory.EnumerateDirectories(dir))
        {
            try
            {
                Directory.Delete(directory, true);
            }
            catch (IOException e)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Failed to clean previous compiler cache");
                Console.ResetColor();
                Console.WriteLine(e);
            }
        }
    }
}
