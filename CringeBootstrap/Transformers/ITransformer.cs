using System.Collections.Immutable;
using System.Reflection;
using dnlib.DotNet;

namespace CringeBootstrap.Transformers;

public interface ITransformer
{
    ImmutableArray<AssemblyName> AcceptedAssemblies { get; }

    bool Transform(TransformationContext context);
    
    // todo add a way to force invalidate the assembly instead of waiting for global cache invalidation
}