#if !WINDOWS
using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.InteropServices;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using CallingConvention = dnlib.DotNet.CallingConvention;
using FieldAttributes = dnlib.DotNet.FieldAttributes;
using MethodAttributes = dnlib.DotNet.MethodAttributes;
using TypeAttributes = dnlib.DotNet.TypeAttributes;

namespace CringeBootstrap.Transformers.Impl;

internal class DllImportTransformer : ITransformer
{
    // contract with the resolver
    public const string EntrypointModuleName = "CringeBootstrap.Native.so";
    
    internal const MethodAttributes EntrypointHelperAttributes = MethodAttributes.Assembly | MethodAttributes.HideBySig | MethodAttributes.Static;
    internal const MethodAttributes EntrypointInteropAttributes = EntrypointHelperAttributes | MethodAttributes.PinvokeImpl;

    internal const PInvokeAttributes ImplMapAttributes =
        PInvokeAttributes.CallConvStdCall | PInvokeAttributes.CharSetUnicode |
        PInvokeAttributes.ThrowOnUnmappableCharEnabled | PInvokeAttributes.BestFitDisabled |
        PInvokeAttributes.NoMangle;

    private const string HavokConstraintType = "Havok.HkConstraint";
    
    private const string HavokWrapper = "HavokWrapper";
    private const string HavokWrapperModule = $"{HavokWrapper}.dll";
    private const string VRageNativeWrapper = "VRage.NativeWrapper";
    private const string VRageNativeWrapperModule = $"{VRageNativeWrapper}.dll";
    private const string PInvokeCallbackAttribute = "MonoPInvokeCallbackAttribute";

    public ImmutableArray<AssemblyName> AcceptedAssemblies { get; } =
        [
            new($"{HavokWrapper}, Version=0.1.1.0, Culture=neutral, PublicKeyToken=null"),
            new($"{VRageNativeWrapper}, Version=0.1.1.0, Culture=neutral, PublicKeyToken=null"),
        ];
    public bool Transform(TransformationContext context)
    {
        var moduleDefinition = context.Module;
        if (moduleDefinition.Name.String is not (HavokWrapperModule or VRageNativeWrapperModule)) return false;
        
        var methods = CollectInteropMethods(moduleDefinition);
        if (methods.Count == 0)
            throw new InvalidOperationException($"No interop methods found for module {moduleDefinition.Name}");
        // var interopPairs = EmitInteropType(moduleDefinition, methods);
        // EmitInteropMethodBodies(moduleDefinition, interopPairs);
        
        ApplyStructFixUps(moduleDefinition);
        return true;
    }

    private void ApplyStructFixUps(ModuleDefMD module)
    {
        switch (module.Name)
        {
            case HavokWrapperModule:
            {
                ApplyHavokWrapperFixUps(module);
                RewriteReversePInvoke(module);
                break;
            }
        }
    }

    private void ApplyHavokWrapperFixUps(ModuleDefMD module)
    {
        {
            var bufferType = module.FindReflectionThrow("Havok.Utils.HkManagedIntermediateBuffer+Native");

            const string propName = "IsBufferUnmanaged";
            bufferType.FindField(propName, new FieldSig(module.CorLibTypes.Boolean)).FieldType =
                module.CorLibTypes.Byte;
        }

        {
            var constraintType = module.FindReflectionThrow(HavokConstraintType);

            const string getAttachedConstraints = "GetAttachedConstraints";
            var getAttachedConstraintsMethod = constraintType.FindMethod(getAttachedConstraints);
            if (getAttachedConstraintsMethod is null)
                throw new MissingMethodException(HavokConstraintType, getAttachedConstraints);

            var ins = (List<Instruction>)getAttachedConstraintsMethod.Body.Instructions;

            var delIndex = ins.FindIndex(b => b.OpCode == OpCodes.Newobj && ((IMethodDefOrRef)b.Operand).DeclaringType.Name == "ReadConstraintsCallback");
            var delCtor = (IMethodDefOrRef)ins[delIndex].Operand;

            var field = new FieldDefUser("ConstraintsCallback", new(delCtor.DeclaringType.ToTypeSig(false)))
            {
                IsStatic = true,
                IsInitOnly = true,
                Access = FieldAttributes.Private,
            };
            constraintType.Fields.Add(field);

            var method = (IMethodDefOrRef)ins[delIndex - 1].Operand;

            ins[delIndex] = Instruction.Create(OpCodes.Ldsfld, field);
            ins.RemoveRange(delIndex - 2, 2); // ldnull, ldftn

            var ctor = constraintType.FindOrCreateStaticConstructor();
            ins = (List<Instruction>)ctor.Body.Instructions;

            ins.InsertRange(0,
                Instruction.Create(OpCodes.Ldnull),
                Instruction.Create(OpCodes.Ldftn, method),
                Instruction.Create(OpCodes.Newobj, delCtor),
                Instruction.Create(OpCodes.Stsfld, field)
            );

            {
                var readerMethod = constraintType.FindMethod("ConstraintReader");
                ins = (List<Instruction>)readerMethod.Body.Instructions;

                var index = ins.FindIndex(b => b.OpCode == OpCodes.Stloc_1);

                ins.InsertRange(index + 1,
                    Instruction.Create(OpCodes.Ldloca_S, readerMethod.Body.Variables[2]),
                    Instruction.Create(OpCodes.Call, module.Import(typeof(GCHandle).GetMethod(nameof(GCHandle.Free))))
                );
            }
        }

        {
            var profilerType = module.FindReflectionThrow("Havok.HkTaskProfiler");
            var initMethod = profilerType.FindMethod("Init");

            MovePInvokeCallbackInitToStatic(initMethod, profilerType);

            var shapeCallbackType =
                module.FindReflectionThrow("Havok.HkPhantomCallbackShape+PhantomCallbackShapeManagedWrapper");
            var wrapperCtor = shapeCallbackType.FindConstructors().First();
            
            MovePInvokeCallbackInitToStatic(wrapperCtor, shapeCallbackType);
            
            void MovePInvokeCallbackInitToStatic(MethodDef method, TypeDef type)
            {
                var methodIns = (List<Instruction>)method.Body.Instructions;
                var ctorIns = (List<Instruction>)type.FindOrCreateStaticConstructor().Body.Instructions;
            
                for (var i = 0; i < methodIns.Count; i++)
                {
                    var ins = methodIns[i];
                    if (ins.OpCode == OpCodes.Ldftn &&
                        ((IMethodDefOrRef)ins.Operand).CustomAttributes.IsDefined(PInvokeCallbackAttribute))
                    {
                        var field = ((IField)methodIns[i + 2].Operand).ResolveFieldDefThrow();
                        
                        if (!field.IsStatic)
                        {
                            // currently doesnt handle replacements in other methods for perf
                            for (var i1 = 0; i1 < methodIns.Count; i1++)
                            {
                                var instruction = methodIns[i1];
                                if (instruction.OpCode == OpCodes.Ldfld &&
                                    ((IField)instruction.Operand).FullName == field.FullName)
                                {
                                    instruction.OpCode = OpCodes.Ldsfld;
                                    methodIns[i1 - 1] = Instruction.Create(OpCodes.Nop);
                                }
                            }
                            
                            methodIns[i + 2].OpCode = OpCodes.Stsfld;
                            methodIns[i - 2] = Instruction.Create(OpCodes.Nop);
                        }
                        
                        field.IsStatic = true;
                        field.IsInitOnly = true;
                        
                        ctorIns.InsertRange(0, methodIns.GetRange(i - 1, 4));
                        methodIns[i - 1] = Instruction.Create(OpCodes.Nop);
                        methodIns[i] = Instruction.Create(OpCodes.Nop);
                        methodIns[i + 1] = Instruction.Create(OpCodes.Nop);
                        methodIns[i + 2] = Instruction.Create(OpCodes.Nop);
                    }
                }

                methodIns.RemoveAll(b => b.OpCode == OpCodes.Nop);
            }
        }
    }

    private void RewriteReversePInvoke(ModuleDefMD moduleDefinition)
    {
        var marshaller = new TypeDefUser("InteropServices", "Marshaller", moduleDefinition.CorLibTypes.Object.ToTypeDefOrRef())
        {
            Visibility = TypeAttributes.NotPublic,
            IsClass = true,
            Attributes = TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass
        };
        moduleDefinition.Types.Add(marshaller);

        var createMethod = new MethodDefUser("CreateTrampoline",
            MethodSig.CreateStatic(moduleDefinition.CorLibTypes.IntPtr, moduleDefinition.CorLibTypes.IntPtr, moduleDefinition.CorLibTypes.String),
            EntrypointInteropAttributes)
        {
            ImplMap = new ImplMapUser(new ModuleRefUser(moduleDefinition, EntrypointModuleName),
                "CringeBootstrap_CreateTrampoline", (ImplMapAttributes & ~PInvokeAttributes.CharSetUnicode) | PInvokeAttributes.CharSetAnsi)
        };
        marshaller.Methods.Add(createMethod);
        
        var marshalMethod =
            (IMethodDefOrRef)moduleDefinition.Import(typeof(Marshal).GetMethod(nameof(Marshal.GetFunctionPointerForDelegate),
                [Type.MakeGenericMethodParameter(0)]));

        var fieldsList = new List<FieldDef>();
        foreach (var typeDef in moduleDefinition.GetTypes())
        {
            var staticConstructor = typeDef.FindStaticConstructor();
            if (staticConstructor is null) continue;

            var instructions = (List<Instruction>)staticConstructor.Body.Instructions;

            for (var index = 0; index < instructions.Count; index++)
            {
                var instruction = instructions[index];
                if (instruction.OpCode != OpCodes.Ldftn) continue;
                var methodDef = ((IMethodDefOrRef)instruction.Operand).ResolveMethodDefThrow();
                var attribute = methodDef.CustomAttributes.Find(PInvokeCallbackAttribute);
                if (attribute is null) continue;
                
                if (!methodDef.IsStatic)
                    throw new NotSupportedException($"Unsupported method: {methodDef.Name}");

                var delegateType = (ClassSig)attribute.ConstructorArguments[0].Value;

                var setFieldInstruction = instructions[index + 2];
                if (setFieldInstruction.OpCode != OpCodes.Stsfld)
                    throw new InvalidOperationException($"Expected delegate field got {setFieldInstruction}");

                var field = ((IField)setFieldInstruction.Operand).ResolveFieldDefThrow();
                fieldsList.Add(field);

                string? paramName = null;
                foreach (var typeDefMethod in typeDef.Methods)
                {
                    if (!typeDefMethod.HasBody) continue;
                    var foundField = -1;
                    for (var i = 0; i < typeDefMethod.Body.Instructions.Count; i++)
                    {
                        var bodyInstruction = typeDefMethod.Body.Instructions[i];
                        if (foundField != -1 && bodyInstruction.OpCode == OpCodes.Call && bodyInstruction.Operand is IMethodDefOrRef operand)
                        {
                            var callMethod = operand.ResolveMethodDefThrow();
                            if (callMethod.Attributes.HasFlag(MethodAttributes.PinvokeImpl))
                            {
                                var parameter = callMethod.Parameters.Where(b => b.Type.FullName == delegateType.FullName).Reverse().Skip(foundField).First();
                                paramName = parameter.Name;
                            }
                        }
                        else if (bodyInstruction.OpCode == OpCodes.Ldsfld)
                        {
                            if (((IField)bodyInstruction.Operand).FullName == field.FullName)
                                foundField = 0;
                            else if (foundField != -1 && ((IField)bodyInstruction.Operand).FieldSig.Type.FullName == delegateType.FullName)
                                foundField++;
                        }
                    }
                }

                if (paramName is null)
                    throw new NotSupportedException($"Cant find usage for {field} for {methodDef}");

                // prevent angry GC on delegate inst
                var holderField = new FieldDefUser($"{field.Name}Holder", new(delegateType))
                {
                    IsStatic = true,
                    IsInitOnly = true,
                    Access = FieldAttributes.Assembly
                };
                typeDef.Fields.Add(holderField);

                instructions.InsertRange(index + 2,
                    Instruction.Create(OpCodes.Dup),
                    Instruction.Create(OpCodes.Stsfld, holderField),
                    Instruction.Create(OpCodes.Call,
                        new MethodSpecUser(marshalMethod, new(delegateType))),
                    Instruction.Create(OpCodes.Ldstr, $"{delegateType.ReflectionFullName}_{paramName}"),
                    Instruction.Create(OpCodes.Call, createMethod)
                );
            }
        }
        
        foreach (var field in fieldsList)
        {
            // defer field type changes
            field.FieldType = moduleDefinition.CorLibTypes.IntPtr;
        }
        
        foreach (var interopMethod in CollectInteropMethods(moduleDefinition))
        {
            for (var index = 0; index < interopMethod.Parameters.Count; index++)
            {
                var parameter = interopMethod.Parameters[index];
                if (!IsDel(parameter)) continue;

                parameter.Type = moduleDefinition.CorLibTypes.IntPtr;
                interopMethod.MethodSig.Params[index] = moduleDefinition.CorLibTypes.IntPtr;
            }
        }

        bool IsDel(Parameter parameter)
        {
            var classSig = parameter.Type.ToClassSig();
            return classSig is not null && IsDelSig(classSig);
        }

        bool IsDelSig(TypeSig sig)
        {
            return sig.ToTypeDefOrRef().GetBaseTypeThrow().ReflectionFullName is "System.MulticastDelegate";
        }
    }

    private void EmitInteropMethodBodies(ModuleDefMD moduleDefinition, Dictionary<MethodDef, FieldInfo> interopPairs)
    {
        TypeDef? marshallerType = null;
        var marshallers = new Dictionary<(NativeType, bool reverse), MethodDef>();
        
        foreach (var (method, (field, fnPtrSig)) in interopPairs)
        {
            method.IsPinvokeImpl = false;
            method.IsPreserveSig = false;
            method.ImplMap = null;
            var fnPtrLocal = new Local(field.FieldType);
            method.Body = new CilBody
            {
                InitLocals = true,
                MaxStack = 8,
                Variables =
                {
                    fnPtrLocal
                },
                Instructions =
                {
                    OpCodes.Ldsfld.ToInstruction(field),
                    OpCodes.Stloc.ToInstruction(fnPtrLocal)
                }
            };
            var instructions = method.Body.Instructions;

            foreach (var parameter in method.Parameters)
            {
                var opCode = parameter.Type.IsByRef && !parameter.ParamDef.HasFieldMarshal
                    ? OpCodes.Ldarga
                    : OpCodes.Ldarg;
                instructions.Add(opCode.ToInstruction(parameter));
                if (!parameter.ParamDef.HasFieldMarshal) continue;

                var marshaller = EmitMarshaller(moduleDefinition, parameter.ParamDef.MarshalType, false,
                    ref marshallerType, marshallers);

                if (parameter.Type.IsByRef)
                {
                    var byRefLocal = new Local(marshaller.ReturnType);
                    method.Body.Variables.Add(byRefLocal);

                    instructions.Add(OpCodes.Call.ToInstruction(marshaller));
                    instructions.Add(OpCodes.Stloc.ToInstruction(byRefLocal));
                    instructions.Add(OpCodes.Ldloca.ToInstruction(byRefLocal));
                }
                else
                {
                    instructions.Add(OpCodes.Call.ToInstruction(marshaller));
                }
            }
            
            instructions.Add(OpCodes.Ldloc.ToInstruction(fnPtrLocal));
            instructions.Add(OpCodes.Calli.ToInstruction(fnPtrSig.MethodSig));

            var returnParameter = method.Parameters.ReturnParameter;
            
            if (returnParameter.ParamDef?.HasFieldMarshal ?? false)
            {
                if (returnParameter.Type.IsByRef)
                    throw new NotSupportedException($"Unsupported return parameter type: {returnParameter.Type}");
                
                var marshaller = EmitMarshaller(moduleDefinition, returnParameter.ParamDef.MarshalType, true, ref marshallerType, marshallers);
                
                instructions.Add(OpCodes.Call.ToInstruction(marshaller));
            }
            
            var index = 0;
            Local? returnLocal = null;
            if (method.HasReturnType)
            {
                returnLocal = new Local(returnParameter.Type);
                method.Body.Variables.Add(returnLocal);
            
                instructions.Add(OpCodes.Stloc.ToInstruction(returnLocal));
                index = 1;
            }
            
            foreach (var parameter in method.Parameters)
            {
                if (!parameter.Type.IsByRef || !(parameter.ParamDef?.HasFieldMarshal ?? false))
                    continue;
                
                var marshaller = EmitMarshaller(moduleDefinition, parameter.ParamDef.MarshalType, true, ref marshallerType, marshallers);
                
                instructions.Add(OpCodes.Ldarg.ToInstruction(parameter));
                instructions.Add(OpCodes.Ldloc.ToInstruction(method.Body.Variables[index++]));
                instructions.Add(OpCodes.Call.ToInstruction(marshaller));
                instructions.Add(OpCodes.Stind_I1.ToInstruction());
            }
            
            if (method.HasReturnType)
                instructions.Add(OpCodes.Ldloc.ToInstruction(returnLocal));
            instructions.Add(OpCodes.Ret.ToInstruction());
        }
        
        if (marshallerType is not null)
            moduleDefinition.Types.Add(marshallerType);
    }

    private MethodDef EmitMarshaller(ModuleDefMD module, MarshalType marshalType, bool reverse, ref TypeDef? marshallerType, Dictionary<(NativeType, bool reverse), MethodDef> cache)
    {
        marshallerType ??= new TypeDefUser("InteropServices", "Marshaller", module.CorLibTypes.Object.ToTypeDefOrRef())
        {
            Visibility = TypeAttributes.NotPublic,
            IsClass = true,
            Attributes = TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass
        };
        
        if (cache.TryGetValue((marshalType.NativeType, reverse), out var methodDef))
            return methodDef;
        
        if (marshalType.NativeType is not (NativeType.I1 or NativeType.U1)) throw new NotSupportedException($"Unsupported marshal type: {marshalType}");

        if (reverse)
        {
            methodDef = new MethodDefUser($"Marshal{marshalType.NativeType}Reverse",
                MethodSig.CreateStatic(module.CorLibTypes.Boolean, module.CorLibTypes.Byte),
                MethodAttributes.Public | MethodAttributes.Static);
        }
        else
        {
            methodDef = new MethodDefUser($"Marshal{marshalType.NativeType}",
                MethodSig.CreateStatic(module.CorLibTypes.Byte, module.CorLibTypes.Boolean),
                MethodAttributes.Public | MethodAttributes.Static);
        }
        marshallerType.Methods.Add(methodDef);
        
        var retStatement = Instruction.CreateLdcI4(0);
        methodDef.Body = new CilBody
        {
            InitLocals = true,
            MaxStack = 8,
            Instructions =
            {
                OpCodes.Ldarg_0.ToInstruction(),
                OpCodes.Brfalse.ToInstruction(retStatement),
                Instruction.CreateLdcI4(1),
                OpCodes.Ret.ToInstruction(),
                retStatement,
                OpCodes.Ret.ToInstruction()
            }
        };
        
        cache.Add((marshalType.NativeType, reverse), methodDef);
        return methodDef;
    }

    private Dictionary<MethodDef, FieldInfo> EmitInteropType(ModuleDefMD module, List<MethodDef> interopMethods)
    {
        var typeDef = new TypeDefUser("InteropServices", "NativeMethods", module.CorLibTypes.Object.ToTypeDefOrRef())
        {
            Visibility = TypeAttributes.NotPublic,
            IsClass = true,
            Attributes = TypeAttributes.Sealed | TypeAttributes.AutoClass | TypeAttributes.AnsiClass
        };
        module.Types.Add(typeDef);
        
        var entrypointRef = new ModuleRefUser(module, EntrypointModuleName);
        var lastError = EmitLastErrorMethod(module, typeDef, entrypointRef);
        var getProc = EmitGetProcMethod(module, typeDef, lastError, entrypointRef);

        var constructor = typeDef.FindOrCreateStaticConstructor();
        var constructorBody = constructor.Body;
        var instructions = constructorBody.Instructions;
        instructions.Clear();

        var hModuleLocal = new Local(module.CorLibTypes.IntPtr);
        constructorBody.Variables.Add(hModuleLocal);

        var dllSearchPathRef = module.Import(typeof(DllImportSearchPath?));
        var searchPathLocal = new Local(dllSearchPathRef.ToTypeSig());
        constructorBody.Variables.Add(searchPathLocal);
        
        instructions.Add(OpCodes.Ldloca.ToInstruction(searchPathLocal));
        instructions.Add(OpCodes.Initobj.ToInstruction(dllSearchPathRef));

        var libName = interopMethods[0].ImplMap.Module.Name;
        instructions.Add(OpCodes.Ldstr.ToInstruction(libName));
        instructions.Add(
            OpCodes.Call.ToInstruction(
                module.Import(typeof(Assembly).GetMethod(nameof(Assembly.GetExecutingAssembly)))));
        instructions.Add(OpCodes.Ldloc.ToInstruction(searchPathLocal));
        instructions.Add(OpCodes.Call.ToInstruction(module.Import(
            typeof(NativeLibrary).GetMethod(nameof(NativeLibrary.Load),
                [typeof(string), typeof(Assembly), typeof(DllImportSearchPath?)]))));
        instructions.Add(OpCodes.Stloc.ToInstruction(hModuleLocal));

        var dictionary = interopMethods.ToDictionary(b => b, interopMethod =>
        {
            var args = new List<TypeSig>(interopMethod.Parameters.Count);
            
            foreach (var parameter in interopMethod.Parameters)
            {
                if (parameter.ParamDef.HasFieldMarshal)
                {
                    switch (parameter.ParamDef.MarshalType.NativeType)
                    {
                        case NativeType.I1 or NativeType.U1:
                            args.Add(module.CorLibTypes.Byte);
                            break;
                        default:
                            throw new NotSupportedException($"Unsupported marshal type: {parameter.ParamDef.MarshalType}");
                    }

                    continue;
                }
                
                args.Add(parameter.Type);
            }

            var returnType = interopMethod.Parameters.ReturnParameter.ParamDef?.HasFieldMarshal ?? false
                ? interopMethod.Parameters.ReturnParameter.ParamDef.MarshalType.NativeType is NativeType.I1 or NativeType.U1
                    ? module.CorLibTypes.Byte
                    : throw new NotSupportedException(
                        $"Unsupported marshal type: {interopMethod.Parameters.ReturnParameter.ParamDef.MarshalType}")
                : interopMethod.ReturnType;
            
            var methodSig = new MethodSig(CallingConvention.StdCall, 0,
                returnType, args);
            var fnPtrSig = new FnPtrSig(methodSig);
            var fieldDef = new FieldDefUser(interopMethod.Name, new FieldSig(module.Import(fnPtrSig)),
                FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.InitOnly);
            typeDef.Fields.Add(fieldDef);

            instructions.Add(OpCodes.Ldloc.ToInstruction(hModuleLocal));
            instructions.Add(OpCodes.Ldstr.ToInstruction(interopMethod.ImplMap.Name));
            instructions.Add(OpCodes.Call.ToInstruction(getProc));
            instructions.Add(OpCodes.Stsfld.ToInstruction(fieldDef));
            
            return new FieldInfo(fieldDef, fnPtrSig);
        });
        
        instructions.Add(OpCodes.Ret.ToInstruction());
        
        return dictionary;
    }

    private MethodDef EmitLastErrorMethod(ModuleDefMD module, TypeDefUser typeDef, ModuleRef entrypointRef)
    {
        var lastErrorSig = MethodSig.CreateStatic(module.CorLibTypes.Int32);
        var lastError = new MethodDefUser("WinLastError", lastErrorSig, EntrypointInteropAttributes)
        {
            ImplMap = new ImplMapUser(entrypointRef, "WinLastError", ImplMapAttributes)
        };
        typeDef.Methods.Add(lastError);

        return lastError;
    }

    private MethodDef EmitGetProcMethod(ModuleDefMD module, TypeDefUser typeDef, MethodDef lastErrorMethod, ModuleRef entrypointRef)
    {
        var getProcInteropSig = MethodSig.CreateStatic(module.CorLibTypes.IntPtr, module.CorLibTypes.IntPtr, module.CorLibTypes.String);
        var getProcInterop = new MethodDefUser("WinGetProcAddress", getProcInteropSig, EntrypointInteropAttributes)
            {
                ImplMap = new ImplMapUser(entrypointRef, "WinGetProcAddress", ImplMapAttributes)
            };
        typeDef.Methods.Add(getProcInterop);

        getProcInterop.Parameters[1].CreateParamDef();
        var nameParamDef = getProcInterop.Parameters[1].ParamDef;
        nameParamDef.HasFieldMarshal = true;
        nameParamDef.MarshalType = new MarshalType(NativeType.LPStr);

        var getProcSig = MethodSig.CreateStatic(module.CorLibTypes.IntPtr, module.CorLibTypes.IntPtr, module.CorLibTypes.String);
        var getProc = new MethodDefUser("GetProcAddress", getProcSig, EntrypointHelperAttributes);
        typeDef.Methods.Add(getProc);
        
        var fnPtrLocal = new Local(module.CorLibTypes.IntPtr);
        var retStatement = OpCodes.Ldloc.ToInstruction(fnPtrLocal);
        getProc.Body = new CilBody
        {
            InitLocals = true,
            MaxStack = 8,
            Variables =
            {
                fnPtrLocal
            },
            Instructions =
            {
                OpCodes.Ldarg_0.ToInstruction(),
                OpCodes.Ldarg_1.ToInstruction(),
                OpCodes.Call.ToInstruction(getProcInterop),
                OpCodes.Stloc.ToInstruction(fnPtrLocal),
                OpCodes.Ldloc.ToInstruction(fnPtrLocal),
                OpCodes.Brtrue.ToInstruction(retStatement),
                OpCodes.Call.ToInstruction(lastErrorMethod),
                OpCodes.Call.ToInstruction(
                    module.Import(typeof(Marshal).GetMethod(nameof(Marshal.GetExceptionForHR), [typeof(int)]))),
                OpCodes.Throw.ToInstruction(),
                retStatement,
                OpCodes.Ret.ToInstruction(),
            }
        };
        
        return getProc;
    }

    private List<MethodDef> CollectInteropMethods(ModuleDefMD module)
    {
        return module.GetTypes().SelectMany(t => t.Methods)
            .Where(m => m.Attributes.HasFlag(MethodAttributes.PinvokeImpl))
            .ToList();
    }

    private record struct FieldInfo(FieldDef Def, FnPtrSig Sig);
}
#endif
