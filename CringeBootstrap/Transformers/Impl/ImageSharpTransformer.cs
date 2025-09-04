using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NLog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Memory;

namespace CringeBootstrap.Transformers.Impl;

internal class ImageSharpTransformer : ITransformer
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    
    public ImmutableArray<AssemblyName> AcceptedAssemblies { get; } =
        [new("VRage.Render, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null")];
    public bool Transform(ModuleDefMD moduleDefinition)
    {
        if (!PatchStaticCtor(moduleDefinition)) return false;
        if (!PatchGenericType(moduleDefinition)) return false;

        return true;
    }

    private static bool PatchGenericType(ModuleDefMD moduleDefinition)
    {
        var typeDefinition = moduleDefinition.Find("VRage.Render.Image.MyImage`1", true);
        if (typeDefinition is null) return false;

        var method = typeDefinition.FindMethods("Create").FirstOrDefault(b => b.Parameters[0].Name == "stream");

        if (method is null)
        {
            Log.Error("Couldnt find create method in myimage");
            return false;
        }

        var imageType =
            new TypeSpecUser(new GenericInstSig(moduleDefinition.ImportAsTypeSig(typeof(Image<>)).ToClassOrValueTypeSig(),
                new GenericMVar(0, method))).ResolveTypeDefThrow();

        var body = method.Body;
        
        for (var i = 4; i < body.Instructions.Count; i++)
        {
            var instruction = body.Instructions[i];
            body.Instructions.RemoveAt(i--);
            if (instruction.IsStloc() && instruction.GetLocal(body.Variables).Index == 2)
                break;
        }
        
        var advancedExtensionsType = moduleDefinition.Import(typeof(AdvancedImageExtensions)).ResolveTypeDefThrow();
        var memoryGroupTypeSig = new GenericInstSig(moduleDefinition.ImportAsTypeSig(typeof(IMemoryGroup<>)).ToClassOrValueTypeSig(),
            new GenericMVar(0));
        var memoryGroupTypeDef = new TypeSpecUser(memoryGroupTypeSig).ResolveTypeDefThrow();
        var getPixelGroupMethod = advancedExtensionsType.FindMethods("GetPixelMemoryGroup")
            .First(b => b.Parameters[0].Type.TypeName == "Image`1");
        body.Instructions.Insert(4,
            Instruction.Create(OpCodes.Call,
                new MethodSpecUser(moduleDefinition.Import(getPixelGroupMethod), new GenericInstMethodSig(new GenericMVar(0, method)))));
        body.Instructions.Insert(5, Instruction.Create(OpCodes.Dup));
        var getMethod = memoryGroupTypeDef.FindProperty("TotalLength").GetMethod;
        body.Instructions.Insert(6,
            Instruction.Create(OpCodes.Callvirt,
                new MemberRefUser(getMethod.Module, getMethod.Name, getMethod.MethodSig, new TypeSpecUser(memoryGroupTypeSig))));
        body.Instructions.Insert(7, Instruction.Create(OpCodes.Newarr, new TypeSpecUser(new GenericVar(0))));
        body.Instructions.Insert(8, Instruction.Create(OpCodes.Stloc_2));
        body.Instructions.Insert(9, Instruction.Create(OpCodes.Ldloc_2));
        var spanSig = new GenericInstSig(moduleDefinition.ImportAsTypeSig(typeof(Span<>)).ToClassOrValueTypeSig(), new GenericMVar(0, method));
        var spanCtor = new TypeSpecUser(spanSig).ResolveTypeDefThrow().FindInstanceConstructors()
            .First(b => b.Parameters is [.., { Name: "array" }]);
        body.Instructions.Insert(10,
            Instruction.Create(OpCodes.Newobj,
                new MemberRefUser(spanCtor.Module, spanCtor.Name, spanCtor.MethodSig,
                    new TypeSpecUser(new GenericInstSig(
                        moduleDefinition.ImportAsTypeSig(typeof(Span<>)).ToClassOrValueTypeSig(),
                        new GenericVar(0))))));
        var castMethod = moduleDefinition.Import(typeof(MemoryMarshal)).ResolveTypeDefThrow()
            .FindMethod("Cast",
                MethodSig.CreateStaticGeneric(2,
                    new GenericInstSig(moduleDefinition.ImportAsTypeSig(typeof(Span<>)).ToClassOrValueTypeSig(),
                        new GenericMVar(1)),
                    new GenericInstSig(moduleDefinition.ImportAsTypeSig(typeof(Span<>)).ToClassOrValueTypeSig(),
                        new GenericMVar(0))));
        Debug.Assert(castMethod is not null);
        body.Instructions.Insert(11,
            Instruction.Create(OpCodes.Call,
                new MethodSpecUser(moduleDefinition.Import(castMethod),
                    new GenericInstMethodSig(new GenericVar(0), new GenericMVar(0)))));
        var memoryGroupExtensionsType = moduleDefinition
            .Import(Type.GetType("SixLabors.ImageSharp.Memory.MemoryGroupExtensions, SixLabors.ImageSharp"))
            .ResolveTypeDefThrow();
        var copyToMethod = memoryGroupExtensionsType.FindMethod("CopyTo",
            MethodSig.CreateStaticGeneric(1, moduleDefinition.CorLibTypes.Void, memoryGroupTypeSig,
                new GenericInstSig(moduleDefinition.ImportAsTypeSig(typeof(Span<>)).ToClassOrValueTypeSig(),
                    new GenericMVar(0))));
        Debug.Assert(copyToMethod is not null);
        body.Instructions.Insert(12,
            Instruction.Create(OpCodes.Call,
                new MethodSpecUser(moduleDefinition.Import(copyToMethod), new GenericInstMethodSig(new GenericMVar(0)))));

        moduleDefinition.Assembly.CustomAttributes.Add(new CustomAttribute(moduleDefinition.Import(moduleDefinition
                .Import(Type.GetType("System.Runtime.CompilerServices.IgnoresAccessChecksToAttribute, MonoMod.Utils"))
                .ResolveTypeDefThrow().FindInstanceConstructors().First()),
            [new CAArgument(moduleDefinition.CorLibTypes.String, "SixLabors.ImageSharp")]));
        return true;
    }

    private static bool PatchStaticCtor(ModuleDefMD moduleDefinition)
    {
        var typeDefinition = moduleDefinition.Find("VRage.Render.Image.MyImage", true);
        if (typeDefinition is null) return false;

        if (typeDefinition.FindStaticConstructor() is { Body: { } body })
        {
            body.Instructions.Clear();
            body.Instructions.Add(Instruction.Create(OpCodes.Ret));
        }

        return true;
    }
}