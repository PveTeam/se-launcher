using System.Collections.Immutable;
using System.Reflection;

namespace SharedCringe.Abstractions.Transformers;

public interface ITransformer
{
    ImmutableArray<AssemblyName> AcceptedAssemblies { get; }

    bool Transform(TransformationContext context);
    
    // todo add a way to force invalidate the assembly instead of waiting for global cache invalidation
}
