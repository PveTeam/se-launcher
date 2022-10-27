using System.Reflection;

namespace CringeBootstrap.Abstractions;

public interface ICoreLoadContext
{
    Assembly? ResolveFromAssemblyName(AssemblyName assemblyName);
    nint ResolveUnmanagedDll(string unmanagedDllName);
}