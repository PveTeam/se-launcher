using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;

namespace SharedCringe.Loader;

public abstract class DerivedAssemblyLoadContext : AssemblyLoadContext
{
    protected readonly ICoreLoadContext ParentContext;

    protected DerivedAssemblyLoadContext(ICoreLoadContext parentContext, string name) : base(name, true)
    {
        ParentContext = parentContext;
        AlcMapper.Add(this);
    }

    protected override Assembly? Load(AssemblyName assemblyName) => ParentContext.ResolveFromAssemblyName(assemblyName);
    protected override nint LoadUnmanagedDll(string unmanagedDllName) => ParentContext.ResolveUnmanagedDll(unmanagedDllName);
}
