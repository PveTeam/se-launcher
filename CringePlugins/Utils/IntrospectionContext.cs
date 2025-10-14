using System.Reflection;
using System.Runtime.CompilerServices;
using dnlib.DotNet;
using VRage.FileSystem;

namespace CringePlugins.Utils;

public class IntrospectionContext
{
    public static IntrospectionContext Global { get; } = new();

    private readonly ConditionalWeakTable<Module, ModuleDefMD> _moduleDefCache = new();

    internal readonly ModuleContext Context;

    public IntrospectionContext()
    {
        var assemblyResolver = new AssemblyResolver();

        assemblyResolver.PreSearchPaths.Add(AppContext.BaseDirectory);
        assemblyResolver.PreSearchPaths.Add(MyFileSystem.ExePath);

        Context = new(assemblyResolver);
    }

    internal ModuleDefMD Load(Module module)
    {
        return _moduleDefCache.GetValue(module, LoadDefUncached);
    }
    
    private ModuleDefMD LoadDefUncached(Module module) => ModuleDefMD.Load(module, Context);

    public IEnumerable<Type> CollectAttributedTypes<TAttribute>(Module module, bool allowAbstract = false) where TAttribute : Attribute
    {
        return CollectAttributedTypeDefinitions<TAttribute>(Load(module), allowAbstract)
            .Select(b => module.GetType(b.ClrFullName, true, false)!);
    }

    public IEnumerable<Type> CollectDerivedTypes<T>(Module module, bool allowAbstract = false)
    {
        return CollectDerivedTypeDefinitions<T>(Load(module), allowAbstract)
            .Select(b => module.GetType(b.ClrFullName, true, false)!);
    }

    internal IEnumerable<TypeDef> CollectAttributedTypeDefinitions<TAttribute>(ModuleDef moduleDef, bool allowAbstract = false)
        where TAttribute : Attribute
    {
        var token = moduleDef.ImportAsTypeSig(typeof(TAttribute));

        return moduleDef.GetTypes()
            .Where(b => b.CustomAttributes.Any(a =>
                            a.AttributeType.FullName == token.FullName || MatchBaseType(a.AttributeType, token)) &&
                        (allowAbstract || !b.IsAbstract));
    }

    public IEnumerable<TypeDef> CollectDerivedTypeDefinitions<T>(ModuleDef moduleDef, bool allowAbstract = false)
    {
        var token =  moduleDef.ImportAsTypeSig(typeof(T));

        return moduleDef.GetTypes()
            .Where(b => (typeof(T).IsInterface
                ? b.Interfaces.Any(i => i.Interface.FullName == token.FullName)
                : MatchBaseType(b, token)) && (allowAbstract || !b.IsAbstract));
    }

    private static bool MatchBaseType(ITypeDefOrRef? defOrRef, TypeSig token)
    {
        while ((defOrRef = defOrRef.GetBaseType()) != null)
        {
            if (defOrRef.FullName == token.FullName)
                return true;
        }

        return false;
    }
}

internal static class TypeDefExtensions
{
    extension(TypeDef def)
    {
        public string ClrFullName => def.FullName.Replace('/', '+');
    }
}

public static class AssemblyExtensions
{
    public static Module GetMainModule(this Assembly assembly) => assembly.GetModule(assembly.GetName().Name! + ".dll") ?? assembly.GetModules()[0];
}