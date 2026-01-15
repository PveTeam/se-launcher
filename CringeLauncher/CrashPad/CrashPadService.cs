using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using CringeBootstrap.Abstractions;
using CringeLauncher.Loader;
using CringeLauncher.Utils;
using CringePlugins.Loader;
using HarmonyLib;
using MonoMod.Utils;
using Pillar.Demystifier;
using SharedCringe.Utils;

namespace CringeLauncher.CrashPad;

internal class CrashPadService
{
    private readonly Lock _lock = new();
    private readonly string _nextInfoPath;

    public CrashInformation NextInfo { get; } = new()
    {
        Network = new(),
        Plugins = [],
        ModScripts = [],
        Version = new()
    };
    
    public CrashPadService()
    {
        // logs are always saved to appdata
        var dirPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CringeLauncher",
            "logs");
        Directory.CreateDirectory(dirPath);
        _nextInfoPath = Path.Join(dirPath, $"crash-info-{Environment.ProcessId}.json");
        AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
    }

    public void PullPluginInfo(PluginsLifetime lifetime)
    {
        var installedPlugins = lifetime.Plugins.Select(b => new CrashInformation.InstalledPlugin(b.Metadata.Name,
            b.Metadata.Version.ToString(),
            b.Metadata.Source)
        {
            Exception = b.WrappedInstance?.HasError is true
                ? CaptureExceptionFrame(b.WrappedInstance.LastException)
                : null
        });
        
        NextInfo.Plugins.Clear();
        NextInfo.Plugins.UnionWith(installedPlugins);

        NextInfo.Network.NugetSourceFailed = lifetime.SomeSourcesAreUnavailable;
        
        MarkSavePoint();
    }

    public void RegisterModScript(string modName, bool loadedFromCache, string? compilationError = null)
    {
        using var scope = _lock.EnterScope();
        NextInfo.ModScripts.Add(new CrashInformation.ModScript(modName, loadedFromCache, compilationError));
    }

    public void ClearModScripts()
    {
        using var scope = _lock.EnterScope();
        NextInfo.ModScripts.Clear();
    }

    public void MarkSavePoint()
    {
        using var file = File.Create(_nextInfoPath);
        JsonSerializer.Serialize(file, NextInfo);
    }

    private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        CaptureCurrentThreadException((Exception)e.ExceptionObject);
    }

    public void CaptureCurrentThreadException(Exception exception)
    {
        using var scope = _lock.EnterScope();
        
        NextInfo.UnhandledException = CaptureExceptionInformation(exception, Thread.CurrentThread);
        MarkSavePoint();
    }

    private ExceptionInformation CaptureExceptionInformation(Exception exception, Thread? thread = null) =>
        new(thread is null ? null : CaptureThreadInformation(thread), CaptureExceptionFrame(exception));

    private ExceptionInformation.ExceptionFrame CaptureExceptionFrame(Exception exception)
    {
        ImmutableArray<ExceptionInformation.ExceptionFrame> innerFrames =
            exception is AggregateException { InnerExceptions: var innerExceptions }
                ?
                [..innerExceptions.Select(CaptureExceptionFrame)]
                :
                exception.InnerException is null
                    ? []
                    : [CaptureExceptionFrame(exception.InnerException)];

        return new(CaptureTypeFullName(exception.GetType()), exception.Message,
            CaptureExceptionStackFrames(exception).Result,
            CaptureExceptionStringRepresentation(exception), innerFrames);
    }

    private static string CaptureExceptionStringRepresentation(Exception exception)
    {
        // todo think about if i want to include original stack trace
        // todo if not also remove/restore stacktrace for inner exceptions
        ref var stackTrace = ref ExceptionFormatter.StackTraceField(exception);
        var originalStackTrace = stackTrace;
        try
        {
            stackTrace = null;
            return exception.ToString();
        }
        finally
        {
            stackTrace = originalStackTrace;
        }
    }

    private async Task<ImmutableArray<ExceptionInformation.ExceptionStackFrame>> CaptureExceptionStackFrames(Exception exception)
    {
        var stackTrace = new StackTrace(exception, true);

        var builder = ImmutableArray.CreateBuilder<ExceptionInformation.ExceptionStackFrame>(stackTrace.FrameCount);
        var sb = new StringBuilder();
        var options = new StackTraceOptions(new HarmonyStackFrameMethodResolver(), new PortableDebugSymbolsResolver());

        foreach (var frame in await EnhancedStackTrace.GetFramesAsync(stackTrace, options))
        {
            sb.Clear();

            var method = frame.MethodInfo.MethodBase;
            if (method is null) continue;

            frame.MethodInfo.Append(sb);
            sb.AppendPatchInformation(method);
            
            var stringRepresentation = sb.ToString();
            
            builder.Add(new ExceptionInformation.ExceptionStackFrame(stringRepresentation, CaptureMethodFrame(frame, method)));

            if (ExceptionFormatter.IsLastFrameFromForeignExceptionStackTraceField(frame))
                builder.Add(new ExceptionInformation.LastFrameFromForeignExceptionStackTraceExceptionStackFrame());
        }
        
        return builder.ToImmutable();
    }

    private static ExceptionInformation.MethodFrame CaptureMethodFrame(StackFrame frame, MethodBase method)
    {
        var methodType = method switch
        {
            MethodInfo { Attributes: MethodAttributes.PinvokeImpl } => ExceptionInformation.MethodFrameType.Extern,
            MethodInfo { IsStatic: true } => ExceptionInformation.MethodFrameType.Static,
            MethodInfo => ExceptionInformation.MethodFrameType.Instance,
            ConstructorInfo { IsStatic: true } => ExceptionInformation.MethodFrameType.StaticConstructor,
            ConstructorInfo => ExceptionInformation.MethodFrameType.Constructor,
            _ => ExceptionInformation.MethodFrameType.Unknown
        };

        return new(methodType, CaptureMethodInformation(method), frame.GetILOffset(),
            CaptureMethodFrameFileInfo(frame), CaptureMethodFramePatches(method));
    }

    private static ExceptionInformation.MethodInformation CaptureMethodInformation(MethodBase method)
    {
        var stringRepresentation = new StringBuilder().AppendMethod(method).ToString();
        
        var contextInformation = CaptureAssemblyContextInformation(method.Module.Assembly);
        return new(stringRepresentation, contextInformation, method.Name,
            method.GetRealDeclaringType() is { } declaringType ? CaptureTypeFullName(declaringType) : null,
            method.IsDynamicMethod(), CaptureMethodFrameSignature(method));
    }

    private static ImmutableArray<ExceptionInformation.MethodFramePatch> CaptureMethodFramePatches(MethodBase method)
    {
        if (Harmony.GetPatchInfo(method) is not { } patchInfo) return [];

        var builder = ImmutableArray.CreateBuilder<ExceptionInformation.MethodFramePatch>(patchInfo.Prefixes.Count +
            patchInfo.Postfixes.Count + patchInfo.Transpilers.Count + patchInfo.Finalizers.Count);

        foreach (var prefix in patchInfo.Prefixes)
            builder.Add(new ExceptionInformation.MethodFramePatch(prefix.owner,
                ExceptionInformation.MethodFramePatchType.Prefix, CaptureMethodInformation(prefix.PatchMethod)));

        foreach (var postfix in patchInfo.Postfixes)
            builder.Add(new ExceptionInformation.MethodFramePatch(postfix.owner,
                ExceptionInformation.MethodFramePatchType.Postfix, CaptureMethodInformation(postfix.PatchMethod)));

        foreach (var transpiler in patchInfo.Transpilers)
            builder.Add(new ExceptionInformation.MethodFramePatch(transpiler.owner,
                ExceptionInformation.MethodFramePatchType.Transpiler, CaptureMethodInformation(transpiler.PatchMethod)));

        foreach (var finalizer in patchInfo.Finalizers)
            builder.Add(new ExceptionInformation.MethodFramePatch(finalizer.owner,
                ExceptionInformation.MethodFramePatchType.Finalizer, CaptureMethodInformation(finalizer.PatchMethod)));
        
        return builder.ToImmutable();
    }

    private static ExceptionInformation.MethodFrameFileInfo? CaptureMethodFrameFileInfo(StackFrame frame)
    {
        var fileName = frame.GetFileName();
        if (string.IsNullOrEmpty(fileName)) return null;

        return new ExceptionInformation.MethodFrameFileInfo(fileName, frame.GetFileLineNumber(),
            frame.GetFileColumnNumber());
    }

    private static ExceptionInformation.MethodFrameSignature CaptureMethodFrameSignature(MethodBase method)
    {
        return new(CaptureSignatureType(method is MethodInfo methodInfo ? methodInfo.ReturnType : typeof(void)),
        [
            ..method.GetParameters().Select(b =>
                new ExceptionInformation.MethodFrameSignatureParameter(CaptureSignatureType(b.ParameterType, b.IsOut),
                    b.Name))
        ]);
    }

    private static ExceptionInformation.SignatureType CaptureSignatureType(Type type, bool isByRefOut = false)
    {
        if (type.IsByRef)
        {
            if (isByRefOut)
                return new ExceptionInformation.SignatureTypeOut(CaptureTypeFullName(type),
                    CaptureSignatureType(type.GetElementType()!));
            
            return new ExceptionInformation.SignatureTypeByRef(CaptureTypeFullName(type),
                CaptureSignatureType(type.GetElementType()!));
        }
        
        if (type.IsPointer)
        {
            return new ExceptionInformation.SignatureTypePointer(CaptureTypeFullName(type),
                CaptureSignatureType(type.GetElementType()!));
        }
        
        if (type.IsArray)
        {
            return new ExceptionInformation.SignatureTypeArray(CaptureTypeFullName(type),
                CaptureSignatureType(type.GetElementType()!), type.GetArrayRank());
        }

        if (type.IsPrimitive || type == typeof(string) || type == typeof(object))
        {
            ExceptionInformation.PrimitiveType primitiveType;
            if (type == typeof(void)) primitiveType = ExceptionInformation.PrimitiveType.Void;
            else if (type == typeof(bool)) primitiveType = ExceptionInformation.PrimitiveType.Boolean;
            else if (type == typeof(char)) primitiveType = ExceptionInformation.PrimitiveType.Char;
            else if (type == typeof(sbyte)) primitiveType = ExceptionInformation.PrimitiveType.SByte;
            else if (type == typeof(byte)) primitiveType = ExceptionInformation.PrimitiveType.Byte;
            else if (type == typeof(short)) primitiveType = ExceptionInformation.PrimitiveType.Int16;
            else if (type == typeof(ushort)) primitiveType = ExceptionInformation.PrimitiveType.UInt16;
            else if (type == typeof(int)) primitiveType = ExceptionInformation.PrimitiveType.Int32;
            else if (type == typeof(uint)) primitiveType = ExceptionInformation.PrimitiveType.UInt32;
            else if (type == typeof(long)) primitiveType = ExceptionInformation.PrimitiveType.Int64;
            else if (type == typeof(ulong)) primitiveType = ExceptionInformation.PrimitiveType.UInt64;
            else if (type == typeof(nint)) primitiveType = ExceptionInformation.PrimitiveType.Int;
            else if (type == typeof(nuint)) primitiveType = ExceptionInformation.PrimitiveType.UInt;
            else if (type == typeof(float)) primitiveType = ExceptionInformation.PrimitiveType.Single;
            else if (type == typeof(double)) primitiveType = ExceptionInformation.PrimitiveType.Double;
            else if (type == typeof(decimal)) primitiveType = ExceptionInformation.PrimitiveType.Decimal;
            else if (type == typeof(string)) primitiveType = ExceptionInformation.PrimitiveType.String;
            else primitiveType = ExceptionInformation.PrimitiveType.Object;
            return new ExceptionInformation.PrimitiveSignatureType(CaptureTypeFullName(type), primitiveType);
        }

        if (type.IsEnum)
        {
            return new ExceptionInformation.SignatureTypeEnum(CaptureTypeFullName(type),
                CaptureSignatureType(Enum.GetUnderlyingType(type)));
        }

        if (type.IsGenericType && type.IsConstructedGenericType)
        {
            return new ExceptionInformation.SignatureTypeSpec(CaptureTypeFullName(type),
                CaptureSignatureType(type.GetGenericTypeDefinition()),
                [
                    ..type.GenericTypeArguments.Select<Type, ExceptionInformation.SignatureTypeSpecParameter>(b =>
                        new ExceptionInformation.SignatureTypeSpecConstructedParameter(CaptureSignatureType(b)))
                ]);
        }

        return new ExceptionInformation.SignatureType(CaptureTypeFullName(type));
    }

    private static ExceptionInformation.AssemblyContextInformation CaptureAssemblyContextInformation(Assembly assembly)
    {
        var assemblyLoadContext = AssemblyLoadContext.GetLoadContext(assembly);
            
        return assemblyLoadContext switch
        {
            PluginAssemblyLoadContext pluginContext => new ExceptionInformation.PluginAssemblyContextInformation(
                pluginContext.Name!),
            ICoreLoadContext => new ExceptionInformation.BootstrapAssemblyContextInformation(),
            PbAssemblyLoadContext pbContext => new ExceptionInformation.ProgrammableBlockContextInformation(
                pbContext.Name!),
            ModAssemblyLoadContext => new ExceptionInformation.WorldAssemblyContextInformation(),
            null or not null when assemblyLoadContext == AssemblyLoadContext.Default =>
                new ExceptionInformation.DefaultAssemblyContextInformation(),
            _ => new ExceptionInformation.AssemblyContextInformation(assemblyLoadContext?.Name ?? "Dynamic")
        };;
    }

    private static ExceptionInformation.ThreadInformation CaptureThreadInformation(Thread thread)
    {
        return new(thread.Name, thread.ManagedThreadId, ThreadInformationTracker.GetThreadType(thread));
    }

    private static string CaptureTypeFullName(Type type)
    {
        return type.AssemblyQualifiedName ?? $"{type.FullName ?? type.Name}, {type.Assembly.FullName}";
    }
}