using CringeBootstrap.Abstractions;
using System.Reflection;
using System.Runtime.Loader;
using CringePlugins.Abstractions.Loader;
using SharedCringe.Abstractions.Transformers;

namespace CringePlugins.Loader;
internal class LocalLoadContext(
    ICoreLoadContext parentContext,
    string entrypointPath,
    AssemblyDependencyResolver dependencyResolver,
    ITransformationService transformationService,
    IPluginProvider provider,
    PluginMetadata metadata) : PluginAssemblyLoadContext(parentContext, entrypointPath, dependencyResolver,
    transformationService, provider, metadata)
{
    //use MemoryStream so the file can be written over, and check for .pdb
#if WINDOWS
    protected override Assembly LoadAssemblyFile(string path)
    {
        var pdbFile = Path.ChangeExtension(path, ".pdb");

        return File.Exists(pdbFile)
            ? LoadFromStream(new MemoryStream(File.ReadAllBytes(path)), new MemoryStream(File.ReadAllBytes(pdbFile)))
            : LoadFromStream(new MemoryStream(File.ReadAllBytes(path)));
    }
#endif
}
