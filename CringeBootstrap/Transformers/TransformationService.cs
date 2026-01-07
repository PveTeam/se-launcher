using System.Collections.Immutable;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace CringeBootstrap.Transformers;

internal sealed class TransformationService : ITransformationService
{
    private readonly ILookup<string, ITransformer> _transformers;

    private readonly ModuleContext _context;

    private readonly Dictionary<string, TransformationToken?> _tokens = new(StringComparer.OrdinalIgnoreCase);

    public TransformationService(string gameAssembliesPath, ImmutableArray<ITransformer> transformers)
    {
        _transformers = transformers.SelectMany(x => x.AcceptedAssemblies.Select(a => (x, a)))
            .ToLookup(b => b.a.Name ?? string.Empty, b => b.x, StringComparer.OrdinalIgnoreCase);
        
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
        
        var assemblyName = new AssemblyName(moduleDefinition.Assembly!.FullName).Name ?? string.Empty;
        if (_transformers.Contains(assemblyName))
            token = new TransformationToken(moduleDefinition, _transformers[assemblyName]);
        _tokens.Add(assemblyPath, token);
        return token;
    }

    public void Transform(ITransformationToken token, string targetPath)
    {
        if (token is not TransformationToken transformationToken)
            throw new ArgumentException("Invalid token type", nameof(token));

        transformationToken.Transform(targetPath);
    }

    private class TransformationToken(ModuleDefMD moduleDefinition, IEnumerable<ITransformer> transformers) : ITransformationToken
    {
        public void Transform(string targetPath)
        {
            var writerOptions = new ModuleWriterOptions(moduleDefinition);
            foreach (var transformer in transformers)
            {
                // todo think about if bool return type is useful or not
                transformer.Transform(new(moduleDefinition, writerOptions));
            }

            moduleDefinition.Write(targetPath, writerOptions);
        }
    }
}