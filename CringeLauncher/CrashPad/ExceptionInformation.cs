using System.Collections.Immutable;
using System.Text.Json.Serialization;
using CringeLauncher.Utils;

namespace CringeLauncher.CrashPad;

public record ExceptionInformation(ExceptionInformation.ThreadInformation? Thread, ExceptionInformation.ExceptionFrame TopFrame)
{
    public record ExceptionFrame(
        string TypeName,
        string Message,
        ImmutableArray<ExceptionStackFrame> StackFrames,
        string StringRepresentation,
        ImmutableArray<ExceptionFrame> InnerFrames);
    public record ThreadInformation(string? Name, int ManagedId, ThreadType Type);
    public enum ThreadType
    {
        Normal,
        Background,
        ThreadPool,
        GamePool,
        HavokPool
    }

    [JsonDerivedType(typeof(LastFrameFromForeignExceptionStackTraceExceptionStackFrame), "foreign-boundary")]
    public record ExceptionStackFrame(string StringRepresentation, MethodFrame? Method);

    public record LastFrameFromForeignExceptionStackTraceExceptionStackFrame()
        : ExceptionStackFrame(ExceptionFormatter.EndOfStackTraceFromPreviousLocation, null);

    public record MethodFrame(
        MethodFrameType Type,
        MethodInformation Information,
        int IlOffset,
        MethodFrameFileInfo? FileInfo,
        ImmutableArray<MethodFramePatch> Patches);

    public record MethodInformation(
        string StringRepresentation,
        AssemblyContextInformation DeclaringContext,
        string Name,
        string? DeclaringType,
        bool IsDynamicMethod,
        MethodFrameSignature Signature);

    public record MethodFramePatch(string Owner, MethodFramePatchType Type, MethodInformation PatchMethod);
    
    public enum MethodFramePatchType
    {
        Prefix,
        Postfix,
        Transpiler,
        Finalizer
    }

    public record MethodFrameFileInfo(string Path, int Line, int Column);

    public record MethodFrameSignature(SignatureType ReturnType, ImmutableArray<MethodFrameSignatureParameter> Parameters);

    public record MethodFrameSignatureParameter(SignatureType Type, string? Name);

    [JsonDerivedType(typeof(PrimitiveSignatureType), "primitive")]
    [JsonDerivedType(typeof(SignatureTypeSpec), "spec")]
    [JsonDerivedType(typeof(SignatureTypeByRef), "byref")]
    [JsonDerivedType(typeof(SignatureTypeOut), "out")]
    [JsonDerivedType(typeof(SignatureTypePointer), "pointer")]
    [JsonDerivedType(typeof(SignatureTypeArray), "array")]
    [JsonDerivedType(typeof(SignatureTypeEnum), "enum")]
    public record SignatureType(string Name);
    
    public record PrimitiveSignatureType(string Name, PrimitiveType Type) : SignatureType(Name);
    
    public abstract record SignatureTypeModifier(string Name, SignatureType Type) : SignatureType(Name);
    public record SignatureTypeByRef(string Name, SignatureType Type) : SignatureTypeModifier(Name, Type);
    public record SignatureTypeOut(string Name, SignatureType Type) : SignatureTypeByRef(Name, Type);
    public record SignatureTypePointer(string Name, SignatureType Type) : SignatureTypeModifier(Name, Type);
    public record SignatureTypeArray(string Name, SignatureType Type, int Rank) : SignatureTypeModifier(Name, Type);
    
    public record SignatureTypeEnum(string Name, SignatureType UnderlyingType) : SignatureType(Name);
    
    public record SignatureTypeSpec(string Name, SignatureType Type, ImmutableArray<SignatureTypeSpecParameter> Parameters) : SignatureType(Name);
    
    // todo should i implement var and mVar here? is there a reflection api for it?
    [JsonDerivedType(typeof(SignatureTypeSpecConstructedParameter), "constructed")]
    [JsonDerivedType(typeof(SignatureTypeSpecUnknownParameter), "unknown")]
    public abstract record SignatureTypeSpecParameter;

    public record SignatureTypeSpecConstructedParameter(SignatureType Type) : SignatureTypeSpecParameter;

    public record SignatureTypeSpecUnknownParameter : SignatureTypeSpecParameter;
    
    public enum PrimitiveType
    {
        Void,
        Boolean,
        Char,
        SByte,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Int,
        UInt,
        Single,
        Double,
        Decimal,
        String,
        Object,
    }
    
    public enum MethodFrameType
    {
        Unknown,
        Instance,
        Static,
        Constructor,
        StaticConstructor,
        Extern
    }

    [JsonDerivedType(typeof(DefaultAssemblyContextInformation), "default")]
    [JsonDerivedType(typeof(BootstrapAssemblyContextInformation), "bootstrap")]
    [JsonDerivedType(typeof(PluginAssemblyContextInformation), "plugin")]
    [JsonDerivedType(typeof(WorldAssemblyContextInformation), "world")]
    [JsonDerivedType(typeof(ProgrammableBlockContextInformation), "programmable-block")]
    public record AssemblyContextInformation(string Name);

    public record DefaultAssemblyContextInformation() : AssemblyContextInformation("Default");
    public record BootstrapAssemblyContextInformation() : AssemblyContextInformation("CringeBootstrap");
    public record PluginAssemblyContextInformation(string Name) : AssemblyContextInformation(Name);
    public record WorldAssemblyContextInformation() : AssemblyContextInformation("World Mods Context");

    // todo more information about position of block etc
    public record ProgrammableBlockContextInformation(string Name) : AssemblyContextInformation(Name);
}