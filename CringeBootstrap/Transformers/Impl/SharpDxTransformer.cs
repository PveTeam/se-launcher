#if !WINDOWS
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace CringeBootstrap.Transformers.Impl;

internal class SharpDxTransformer : ITransformer
{
    private const string SharpDxName = "SharpDX";
    public ImmutableArray<AssemblyName> AcceptedAssemblies { get; } =
    [
        new($"{SharpDxName}, Version=4.2.0.0, Culture=neutral, PublicKeyToken=null")
    ];
    public bool Transform(ModuleDefMD moduleDefinition)
    {
        if (moduleDefinition.Assembly.Name != SharpDxName) return false;

        var launcherAssembly = moduleDefinition.Context.AssemblyResolver.Resolve("CringeLauncher", moduleDefinition);
        var fileProviderType = launcherAssembly.FindReflectionThrow("CringeLauncher.Platform.Xplat.LauncherFileProvider");
        var instanceField = moduleDefinition.Import(fileProviderType.FindField("Instance"));
        var normalizePathMethod = moduleDefinition.Import(fileProviderType.FindMethod("NormalizePath"));
        
        TransformNativeFile(moduleDefinition, fileProviderType, instanceField, normalizePathMethod);
        TransformNativeStream(moduleDefinition, instanceField, normalizePathMethod);
        TransformResultDescriptor(moduleDefinition);

        return true;
    }

    private static void TransformResultDescriptor(ModuleDefMD moduleDefinition)
    {
        var type = moduleDefinition.FindReflectionThrow("SharpDX.ResultDescriptor");
        
        var formatMethod = new MethodDefUser("FormatHR",
            MethodSig.CreateStatic(moduleDefinition.CorLibTypes.IntPtr, moduleDefinition.CorLibTypes.Int32),
            DllImportTransformer.EntrypointInteropAttributes)
        {
            ImplMap = new ImplMapUser(new ModuleRefUser(moduleDefinition, DllImportTransformer.EntrypointModuleName),
                "CringeBootstrap_FormatHR", DllImportTransformer.ImplMapAttributes)
        };
        type.Methods.Add(formatMethod);
        
        var getMethod = type.FindMethod("GetDescriptionFromResultCode");
        var ptrLocal = new Local(moduleDefinition.CorLibTypes.IntPtr);
        var strLocal = new Local(moduleDefinition.CorLibTypes.String);
        getMethod.Body = new()
        {
            Variables =
            {
                ptrLocal,
                strLocal
            },
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, formatMethod),
                Instruction.Create(OpCodes.Stloc, ptrLocal),
                
                Instruction.Create(OpCodes.Ldloc, ptrLocal),
                Instruction.Create(OpCodes.Call,
                    moduleDefinition.Import(typeof(Marshal).GetMethod(nameof(Marshal.PtrToStringUni),
                        [typeof(IntPtr)]))),
                Instruction.Create(OpCodes.Stloc, strLocal),
                
                Instruction.Create(OpCodes.Ldloc, ptrLocal),
                Instruction.Create(OpCodes.Call,
                    moduleDefinition.Import(typeof(Marshal).GetMethod(nameof(Marshal.FreeHGlobal),
                        [typeof(IntPtr)]))),
                
                Instruction.Create(OpCodes.Ldloc, strLocal),
                Instruction.Create(OpCodes.Ret)
            }
        };
    }

    private static void TransformNativeStream(ModuleDefMD moduleDefinition, MemberRef instanceField,
        MemberRef normalizePathMethod)
    {
        var type = moduleDefinition.FindReflectionThrow("SharpDX.IO.NativeFileStream");
        var handleField = type.FindField("handle");
        handleField.FieldSig = new(moduleDefinition.ImportAsTypeSig(typeof(FileStream)));

        var ctor = type.FindInstanceConstructors().First();
        ctor.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call,
                    moduleDefinition.Import(typeof(Stream).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, []))),
                
                Instruction.Create(OpCodes.Ldsfld, instanceField),
                Instruction.Create(OpCodes.Ldarga, ctor.Parameters[1]),
                Instruction.Create(OpCodes.Call, normalizePathMethod),
                
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Ldarg_2),
                Instruction.Create(OpCodes.Ldarg_3),
                Instruction.Create(OpCodes.Ldarg, ctor.Parameters[4]),
                Instruction.Create(OpCodes.Call,
                    moduleDefinition.Import(typeof(File).GetMethod(nameof(File.Open),
                        [typeof(string), typeof(FileMode), typeof(FileAccess), typeof(FileShare)]))),
                Instruction.Create(OpCodes.Stfld, handleField),
                
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_3),
                Instruction.CreateLdcI4((int)FileAccess.Read),
                Instruction.Create(OpCodes.And),
                Instruction.CreateLdcI4(0),
                Instruction.Create(OpCodes.Cgt_Un),
                Instruction.Create(OpCodes.Stfld, type.FindField("canRead")),
                
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_3),
                Instruction.CreateLdcI4((int)FileAccess.Write),
                Instruction.Create(OpCodes.And),
                Instruction.CreateLdcI4(0),
                Instruction.Create(OpCodes.Cgt_Un),
                Instruction.Create(OpCodes.Stfld, type.FindField("canWrite")),
                
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.CreateLdcI4(1),
                Instruction.Create(OpCodes.Stfld, type.FindField("canSeek")),
                
                Instruction.Create(OpCodes.Ret)
            }
        };
        
        var flushMethod = type.FindMethod("Flush");
        flushMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetMethod(nameof(FileStream.Flush), []))),
                Instruction.Create(OpCodes.Ret)
            }
        };

        var seekMethod = type.FindMethod("Seek");
        seekMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Ldarg_2),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetMethod(nameof(FileStream.Seek)))),
                Instruction.Create(OpCodes.Ret)
            }
        };

        var setLengthMethod = type.FindMethod("SetLength");
        setLengthMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetMethod(nameof(FileStream.SetLength)))),
                Instruction.Create(OpCodes.Ret)
            }
        };

        var types = moduleDefinition.CorLibTypes;
        
        var readMethod = type.FindMethod("Read", MethodSig.CreateInstance(types.Int32, new SZArraySig(types.Byte), types.Int32, types.Int32));
        readMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Ldarg_2),
                Instruction.Create(OpCodes.Ldarg_3),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetMethod(nameof(FileStream.Read),
                        [typeof(byte[]), typeof(int), typeof(int)]))),
                Instruction.Create(OpCodes.Ret)
            }
        };
        
        var readSpanMethod = type.FindMethod("Read", MethodSig.CreateInstance(types.Int32, types.IntPtr, types.Int32, types.Int32));
        readSpanMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Ldarg_2),
                Instruction.Create(OpCodes.Add),
                Instruction.Create(OpCodes.Ldarg_3),
                Instruction.Create(OpCodes.Newobj,
                    moduleDefinition.Import(
                        typeof(Span<byte>).GetConstructor([typeof(void).MakePointerType(), typeof(int)]))),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetMethod(nameof(FileStream.Read),
                        [typeof(Span<byte>)]))),
                Instruction.Create(OpCodes.Ret)
            }
        };
        
        var writeMethod = type.FindMethod("Write", MethodSig.CreateInstance(types.Void, new SZArraySig(types.Byte), types.Int32, types.Int32));
        writeMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Ldarg_2),
                Instruction.Create(OpCodes.Ldarg_3),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetMethod(nameof(FileStream.Write),
                        [typeof(byte[]), typeof(int), typeof(int)]))),
                Instruction.Create(OpCodes.Ret)
            }
        };
        
        var writeSpanMethod = type.FindMethod("Write", MethodSig.CreateInstance(types.Void, types.IntPtr, types.Int32, types.Int32));
        writeSpanMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Ldarg_2),
                Instruction.Create(OpCodes.Add),
                Instruction.Create(OpCodes.Ldarg_3),
                Instruction.Create(OpCodes.Newobj,
                    moduleDefinition.Import(
                        typeof(ReadOnlySpan<byte>).GetConstructor([typeof(void).MakePointerType(), typeof(int)]))),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetMethod(nameof(FileStream.Write),
                        [typeof(ReadOnlySpan<byte>)]))),
                Instruction.Create(OpCodes.Ret)
            }
        };

        var lengthProperty = type.FindProperty("Length");
        lengthProperty.GetMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetProperty(nameof(FileStream.Length))!.GetGetMethod())),
                Instruction.Create(OpCodes.Ret)
            }
        };

        var positionProperty = type.FindProperty("Position");
        positionProperty.GetMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetProperty(nameof(FileStream.Position))!.GetGetMethod())),
                Instruction.Create(OpCodes.Ret)
            }
        };
        positionProperty.SetMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(FileStream).GetProperty(nameof(FileStream.Position))!.GetSetMethod())),
                Instruction.Create(OpCodes.Ret)
            }
        };

        var disposeMethod = type.FindMethod("Dispose", MethodSig.CreateInstance(types.Void, types.Boolean));
        disposeMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldfld, handleField),
                Instruction.Create(OpCodes.Callvirt,
                    moduleDefinition.Import(typeof(Stream).GetMethod(nameof(Stream.Dispose)))),
                Instruction.Create(OpCodes.Ret)
            }
        };
    }

    private static void TransformNativeFile(ModuleDefMD moduleDefinition, TypeDef fileProviderType, MemberRef instanceField, MemberRef normalizePathMethod)
    {
        var type = moduleDefinition.FindReflectionThrow("SharpDX.IO.NativeFile");
        
        var existsMethod = type.FindMethod("Exists");
        existsMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldsfld, instanceField),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call, moduleDefinition.Import(fileProviderType.FindMethod("FileExists"))),
                Instruction.Create(OpCodes.Ret)
            }
        };

        var lastWriteTimeMethod = type.FindMethod("GetLastWriteTime");
        lastWriteTimeMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldsfld, instanceField),
                Instruction.Create(OpCodes.Ldarga, lastWriteTimeMethod.Parameters[0]),
                Instruction.Create(OpCodes.Call, normalizePathMethod),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call,
                    moduleDefinition.Import(typeof(File).GetMethod(nameof(File.GetLastWriteTime), [typeof(string)]))),
                Instruction.Create(OpCodes.Ret)
            }
        };

        var readAllTextMethod = type.FindMethods("ReadAllText").First(b => b.Parameters.Count == 3);
        readAllTextMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldsfld, instanceField),
                Instruction.Create(OpCodes.Ldarga, readAllTextMethod.Parameters[0]),
                Instruction.Create(OpCodes.Call, normalizePathMethod),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Ldarg_1),
                Instruction.Create(OpCodes.Call,
                    moduleDefinition.Import(typeof(File).GetMethod(nameof(File.ReadAllText), [typeof(string), typeof(Encoding)]))),
                Instruction.Create(OpCodes.Ret)
            }
        };
        
        var readAllBytesMethod = type.FindMethod("ReadAllBytes");
        readAllBytesMethod.Body = new()
        {
            Instructions =
            {
                Instruction.Create(OpCodes.Ldsfld, instanceField),
                Instruction.Create(OpCodes.Ldarga, readAllBytesMethod.Parameters[0]),
                Instruction.Create(OpCodes.Call, normalizePathMethod),
                Instruction.Create(OpCodes.Ldarg_0),
                Instruction.Create(OpCodes.Call,
                    moduleDefinition.Import(typeof(File).GetMethod(nameof(File.ReadAllBytes), [typeof(string)]))),
                Instruction.Create(OpCodes.Ret)
            }
        };
    }
}
#endif
