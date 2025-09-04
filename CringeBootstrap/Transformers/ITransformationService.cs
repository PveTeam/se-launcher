using System.Reflection;

namespace CringeBootstrap.Transformers;

internal interface ITransformationService
{
    ITransformationToken? PrepareTransformation(string assemblyPath);
    void Transform(ITransformationToken token, string targetPath);
}

internal interface ITransformationToken;