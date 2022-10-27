using CringeBootstrap.Abstractions;
using SharedCringe.Loader;

namespace CringeLauncher.Loader;

public class ModAssemblyLoadContext(ICoreLoadContext parentContext)
    : DerivedAssemblyLoadContext(parentContext, "World Mods Context");