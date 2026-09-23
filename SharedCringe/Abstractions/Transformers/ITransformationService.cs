using dnlib.DotNet;
using SharedCringe.Loader;

namespace SharedCringe.Abstractions.Transformers;

public interface ITransformationService
{
    ITransformationTransaction PrepareTransaction(IEnumerable<ITransformer> transformers, DerivedAssemblyLoadContext? dependenciesContext);
    ITransformationToken? PrepareTransformation(ITransformationTransaction transaction, string assemblyPath);
    void Transform(ITransformationToken token, string targetPath);
}

public interface ITransformationToken
{
    ModuleDef ModuleDef { get; }
}

public interface ITransformationTransaction : IDisposable;
