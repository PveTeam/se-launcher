using CringeBootstrap.Abstractions;
using System.Reflection;

namespace CringePlugins.Loader;
internal class LocalLoadContext(ICoreLoadContext parentContext, string entrypointPath) : PluginAssemblyLoadContext(parentContext, entrypointPath)
{
    //use MemoryStream so the file can be written over, and check for .pdb
    protected override Assembly LoadAssemblyFile(string path)
    {
        var pdbFile = Path.ChangeExtension(path, ".pdb");

        return File.Exists(pdbFile)
            ? LoadFromStream(new MemoryStream(File.ReadAllBytes(path)), new MemoryStream(File.ReadAllBytes(pdbFile)))
            : LoadFromStream(new MemoryStream(File.ReadAllBytes(path)));
    }
}
