using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Reflection;
using dnlib.DotNet;

namespace CringeBootstrap.Transformers;

internal sealed class TransformationService : ITransformationService
{
    private readonly FrozenSet<string> _acceptedAssemblies;
    private readonly ImmutableArray<ITransformer> _transformers;

    private readonly ModuleContext _context;

    private readonly Dictionary<string, TransformationToken?> _tokens = new(StringComparer.OrdinalIgnoreCase);

    public TransformationService(string gameAssembliesPath, ImmutableArray<ITransformer> transformers)
    {
        _transformers = transformers;
        _acceptedAssemblies = transformers.SelectMany(x => x.AcceptedAssemblies).Select(b => b.Name ?? string.Empty)
            .Distinct().ToFrozenSet(StringComparer.OrdinalIgnoreCase);
        
        var assemblyResolver = new AssemblyResolver();

        assemblyResolver.PreSearchPaths.Add(Path.GetDirectoryName(typeof(object).Assembly.Location));
        assemblyResolver.PreSearchPaths.Add(AppContext.BaseDirectory);
        assemblyResolver.PreSearchPaths.Add(gameAssembliesPath);

        _context = new(assemblyResolver);
    }
    
    public ITransformationToken? PrepareTransformation(string assemblyPath)
    {
        if (_tokens.TryGetValue(assemblyPath, out var token))
            return token;
        
        ModuleDefMD moduleDefinition;
        try
        {
            moduleDefinition = ModuleDefMD.Load(assemblyPath, _context);
        }
        catch (Exception)
        {
            return null;
        }
        
        var assemblyName = new AssemblyName(moduleDefinition.Assembly!.FullName);
        if (_acceptedAssemblies.Contains(assemblyName.Name ?? string.Empty))
            token = new TransformationToken(moduleDefinition);
        _tokens.Add(assemblyPath, token);
        return token;
    }

    public void Transform(ITransformationToken token, string targetPath)
    {
        if (token is not TransformationToken transformationToken)
            throw new ArgumentException("Invalid token type", nameof(token));

        transformationToken.Transform(targetPath, _transformers);
    }

    private class TransformationToken(ModuleDefMD moduleDefinition) : ITransformationToken
    {
        public void Transform(string targetPath, ImmutableArray<ITransformer> transformers)
        {
            foreach (var transformer in transformers)
            {
                // todo think about if bool return type is useful or not
                transformer.Transform(moduleDefinition);
            }

            moduleDefinition.Write(targetPath);
        }
    }
}