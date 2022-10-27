using System.Reflection;
using dnlib.DotNet;

namespace CringePlugins.Utils;

public class IntrospectionContext
{
    public static IntrospectionContext Global { get; } = new();

    internal readonly ModuleContext Context = ModuleDef.CreateModuleContext();

    public IEnumerable<Type> CollectAttributedTypes<TAttribute>(Module module, bool allowAbstract = false) where TAttribute : Attribute
    {
        var moduleDef = ModuleDefMD.Load(module, Context);

        return moduleDef.GetTypes()
            .Where(b => b.CustomAttributes.IsDefined(typeof(TAttribute).FullName) && (allowAbstract || !b.IsAbstract))
            .Select(b => module.GetType(b.FullName, true, false)!);
    }

    public IEnumerable<Type> CollectDerivedTypes<T>(Module module, bool allowAbstract = false)
    {
        var moduleDef = ModuleDefMD.Load(module, Context);

        var token =  moduleDef.ImportAsTypeSig(typeof(T));

        return moduleDef.GetTypes()
            .Where(b => (typeof(T).IsInterface
                ? b.Interfaces.Any(i => i.Interface.FullName == token.FullName)
                : MatchBaseType(b, token)) && (allowAbstract || !b.IsAbstract))
            .Select(b => module.GetType(b.FullName, true, false)!);
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

public static class AssemblyExtensions
{
    public static Module GetMainModule(this Assembly assembly) => assembly.GetModule(assembly.GetName().Name! + ".dll") ?? assembly.GetModules()[0];
}