using System.Reflection;
using System.Runtime.Loader;
using CringeBootstrap.Abstractions;

namespace SharedCringe.Loader;

public abstract class DerivedAssemblyLoadContext(ICoreLoadContext parentContext, string name)
    : AssemblyLoadContext(name, true)
{
    protected readonly ICoreLoadContext ParentContext = parentContext;
    
    protected override Assembly? Load(AssemblyName assemblyName) => ParentContext.ResolveFromAssemblyName(assemblyName);
    protected override nint LoadUnmanagedDll(string unmanagedDllName) => ParentContext.ResolveUnmanagedDll(unmanagedDllName);
}