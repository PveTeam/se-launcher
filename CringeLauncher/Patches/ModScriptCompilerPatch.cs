namespace CringeLauncher.Patches;

#if false
[HarmonyPatch]
public static class ModScriptCompilerPatch
{
    private static readonly ILogger Log = LogManager.GetCurrentClassLogger();
    private static AssemblyLoadContext _modContext = new(null, true);
    private static readonly HashSet<string> LoadedModAssemblyNames = new();

    private static readonly ConditionalWeakTable<MyProgrammableBlock, AssemblyLoadContext> LoadContexts = new();

    private static readonly FieldInfo InstanceField = AccessTools.Field(typeof(MyProgrammableBlock), "m_instance");
    private static readonly FieldInfo AssemblyField = AccessTools.Field(typeof(MyProgrammableBlock), "m_assembly");
    private static readonly FieldInfo CompilerErrorsField = AccessTools.Field(typeof(MyProgrammableBlock), "m_compilerErrors");

    static ModScriptCompilerPatch()
    {
        MySession.OnUnloaded += OnUnloaded;

        ModWhitelistAnalyzer =
            AccessTools.FieldRefAccess<MyScriptCompiler, DiagnosticAnalyzer>(
                MyScriptCompiler.Static, "m_modApiWhitelistDiagnosticAnalyzer");
        ScriptWhitelistAnalyzer =
            AccessTools.FieldRefAccess<MyScriptCompiler, DiagnosticAnalyzer>(
                MyScriptCompiler.Static, "m_ingameWhitelistDiagnosticAnalyzer");

        MetadataReferences =
            AccessTools.FieldRefAccess<MyScriptCompiler, List<MetadataReference>>(
                MyScriptCompiler.Static, "m_metadataReferences");

        InjectMod = AccessTools.MethodDelegate<Func<CSharpCompilation, SyntaxTree, int, SyntaxTree>>(
            AccessTools.Method(typeof(MyScriptCompiler), "InjectMod"), MyScriptCompiler.Static);
        
        InjectInstructionCounter = AccessTools.MethodDelegate<Func<CSharpCompilation, SyntaxTree, SyntaxTree>>(
            AccessTools.Method(typeof(MyScriptCompiler), "InjectInstructionCounter"), MyScriptCompiler.Static);
        
        EmitDiagnostics = AccessTools.MethodDelegate<Func<CompilationWithAnalyzers, EmitResult, List<Message>, bool, Task<bool>>>(
            AccessTools.Method(typeof(MyScriptCompiler), "EmitDiagnostics"), MyScriptCompiler.Static);

        MakeAssemblyName =
            AccessTools.MethodDelegate<Func<string, string>>(AccessTools.Method(typeof(MyScriptCompiler),
                "MakeAssemblyName"));

        CreateInstanceMethod = AccessTools.Method(typeof(MyProgrammableBlock), "CreateInstance");
        SetDetailedInfoMethod = AccessTools.Method(typeof(MyProgrammableBlock), "SetDetailedInfo");
    }

    private static void OnUnloaded()
    {
        LoadedModAssemblyNames.Clear();
        if (!_modContext.Assemblies.Any())
            return;
        
        _modContext.Unload();
        _modContext = new(null, true);
    }
    
    [HarmonyPatch(typeof(MyProgrammableBlock), "Compile")]
    [HarmonyPrefix]
    private static bool CompilePrefix(MyProgrammableBlock __instance, string program, string storage, bool instantiate,
                                      ref MyProgrammableBlock.ScriptTerminationReason ___m_terminationReason,
                                      MyIngameScriptComponent ___ScriptComponent)
    {
        if (!MySession.Static.EnableIngameScripts || __instance.CubeGrid is {IsPreview: true} or {CreatePhysics: false})
            return false;

        ___m_terminationReason = MyProgrammableBlock.ScriptTerminationReason.None;
        CompileAsync(__instance, program, storage, instantiate, ___ScriptComponent);
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
            CompileAsync(__instance, ___m_compilerErrors, __instance.Description.Text.ToString(), progress).Wait();
        
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

    private static async Task CompileAsync(MyGuiScreenEditor editor, List<string> errors, string program, MyGuiScreenProgress progress)
    {
        var context = new AssemblyLoadContext(null, true);
        var messages = new List<Message>();
        var script = MyVRage.Platform.Scripting.GetIngameScript(program, "Program", nameof(MyGridProgram));
        await CompileAsync(context, MyApiTarget.Ingame, "check", new[] { script }, messages,
            "PB Code Editor");

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
        scriptComponent.NextUpdate = UpdateType.None;
        scriptComponent.NeedsUpdate = MyEntityUpdateEnum.NONE;

        SetDetailedInfoMethod.Invoke(block, new object?[] { "Compiling..." });

        try
        {
            if (LoadContexts.TryGetValue(block, out var context))
            {
                AccessTools.FieldRefAccess<MyProgrammableBlock, IMyGridProgram?>(block, InstanceField) = null;
                AccessTools.FieldRefAccess<MyProgrammableBlock, Assembly?>(block, AssemblyField) = null;
                context.Unload();
            }
               
            LoadContexts.AddOrUpdate(block, context = new(null, true));
               
            var messages = new List<Message>();
            var assembly = await CompileAsync(context, MyApiTarget.Ingame,
                                              $"pb_{block.EntityId}_{Random.Shared.NextInt64()}",
                                              new[]
                                              {
                                                  MyVRage.Platform.Scripting.GetIngameScript(
                                                      program, "Program", nameof(MyGridProgram))
                                              }, messages, $"PB: {block.DisplayName} ({block.EntityId})");
            
            AccessTools.FieldRefAccess<MyProgrammableBlock, Assembly?>(block, AssemblyField) = assembly;

            var errors = AccessTools.FieldRefAccess<MyProgrammableBlock, List<string>>(block, CompilerErrorsField);
            
            errors.Clear();
            errors.AddRange(messages.Select(b => b.Text));

            if (instantiate)
                MySandboxGame.Static.Invoke(
                    () => CreateInstanceMethod.Invoke(block, new object?[] { assembly, errors, storage }),
                    nameof(CompileAsync));
        }
        catch (Exception e)
        {
            SetDetailedInfoMethod.Invoke(block, new object?[] { e.ToString() });
            Log.Error(e);
        }
    }

    private static async Task<Assembly?> CompileAsync(AssemblyLoadContext context, MyApiTarget target,
                                                      string assemblyName, IEnumerable<Script> scripts,
                                                      List<Message> messages, string? friendlyName,
                                                      bool enableDebugInformation = false)
    {
        friendlyName ??= "<No Name>";
        var assemblyFileName = MakeAssemblyName(assemblyName);
        Func<CSharpCompilation, SyntaxTree, SyntaxTree>? syntaxTreeInjector;
        DiagnosticAnalyzer? whitelistAnalyzer;
        switch (target)
        {
            case MyApiTarget.None:
                whitelistAnalyzer = null;
                syntaxTreeInjector = null;
                break;
            case MyApiTarget.Mod:
            {
                var modId = MyModWatchdog.AllocateModId(friendlyName);
                whitelistAnalyzer = ModWhitelistAnalyzer;
                syntaxTreeInjector = (c, st) => InjectMod(c, st, modId);
                
                //skip if name exists already
                if (!LoadedModAssemblyNames.Add(assemblyFileName))
                {
                    Console.WriteLine($"{assemblyFileName} is already loaded, skipping");
                    return null;
                }
                break;
            }
            case MyApiTarget.Ingame:
                syntaxTreeInjector = InjectInstructionCounter;
                whitelistAnalyzer = ScriptWhitelistAnalyzer;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(target), target, "Invalid compilation target");
        }
        var compilation = CreateCompilation(assemblyFileName, scripts);
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
                    newSyntaxTrees = new[] { syntaxTreeInjector(compilation, syntaxTrees[0]) };
                }
                else
                {
                    var compilation1 = compilation;
                    newSyntaxTrees = await Task
                        .WhenAll(syntaxTrees.Select(
                            x => Task.Run(() => syntaxTreeInjector(compilation1, x)))).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
                injectionFailed = true;
            }
            
            if (newSyntaxTrees is not null)
                compilation = compilation.RemoveAllSyntaxTrees().AddSyntaxTrees(newSyntaxTrees);

        }
        CompilationWithAnalyzers? analyticCompilation = null;
        if (whitelistAnalyzer != null)
        {
            analyticCompilation = compilation.WithAnalyzers(ImmutableArray.Create(whitelistAnalyzer));
            compilation = (CSharpCompilation)analyticCompilation.Compilation;
        }

        using var assemblyStream = new MemoryStream();

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
            success = await EmitDiagnostics(analyticCompilation, emitResult, messages, success).ConfigureAwait(false);
            assemblyStream.Seek(0, SeekOrigin.Begin);
            if (injectionFailed) return null;
            if (success)
                return context.LoadFromStream(assemblyStream);

            await EmitDiagnostics(analyticCompilation, compilationWithoutInjection.Emit(assemblyStream), messages,
                false).ConfigureAwait(false);
        }

        return null;
    }

    private static readonly CSharpCompilationOptions CompilationOptions =
        new(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, platform: Platform.X64);
    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.CSharp11, DocumentationMode.None);
    private static readonly DiagnosticAnalyzer ModWhitelistAnalyzer;
    private static readonly DiagnosticAnalyzer ScriptWhitelistAnalyzer;
    private static readonly List<MetadataReference> MetadataReferences;
    private static readonly Func<CSharpCompilation, SyntaxTree, int, SyntaxTree> InjectMod;
    private static readonly Func<CSharpCompilation, SyntaxTree, SyntaxTree> InjectInstructionCounter;
    private static readonly Func<CompilationWithAnalyzers, EmitResult, List<Message>, bool, Task<bool>> EmitDiagnostics;
    private static readonly Func<string, string> MakeAssemblyName;
    private static readonly MethodInfo CreateInstanceMethod;
    private static readonly MethodInfo SetDetailedInfoMethod;
    
    
    private static CSharpCompilation CreateCompilation(string assemblyFile, IEnumerable<Script>? scripts)
    {
        if (scripts == null)
            return CSharpCompilation.Create(assemblyFile, null, MetadataReferences,
                                            CompilationOptions);


        var parseOptions = ParseOptions.WithPreprocessorSymbols(MyScriptCompiler.Static.ConditionalCompilationSymbols);
        var enumerable = scripts.Select(s => CSharpSyntaxTree.ParseText(s.Code, parseOptions, s.Name, Encoding.UTF8));
        
        return CSharpCompilation.Create(assemblyFile, enumerable, MetadataReferences, CompilationOptions);
    }
}
#endif