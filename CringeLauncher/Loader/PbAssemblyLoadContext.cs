using CringeBootstrap.Abstractions;
using SharedCringe.Loader;

namespace CringeLauncher.Loader;
public class PbAssemblyLoadContext(ICoreLoadContext parentContext, string name)
    : DerivedAssemblyLoadContext(parentContext, name);
