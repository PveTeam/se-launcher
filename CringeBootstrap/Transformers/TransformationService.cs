using System.Collections.Immutable;
using System.Reflection;
using CringeBootstrap.Abstractions;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using SharedCringe.Abstractions.Transformers;
using SharedCringe.Loader;
using SharedCringe.Utils;

namespace CringeBootstrap.Transformers;

internal sealed class TransformationService : ITransformationService
{
    private readonly ImmutableArray<string> _assemblyPaths;
    private readonly ModuleContext _defaultContext;

    private readonly Dictionary<string, TransformationToken?> _tokens = new(StringComparer.OrdinalIgnoreCase);

    public TransformationService(string gameAssembliesPath)
    {
        var assemblyResolver = new AssemblyResolver();

        assemblyResolver.PreSearchPaths.Add(Path.GetDirectoryName(typeof(object).Assembly.Location));
        assemblyResolver.PreSearchPaths.Add(AppContext.BaseDirectory);
        assemblyResolver.PreSearchPaths.Add(gameAssembliesPath);
        _assemblyPaths = [.. assemblyResolver.PreSearchPaths];

        _defaultContext = new(assemblyResolver);
    }
    
    public ITransformationTransaction PrepareTransaction(IEnumerable<ITransformer> transformers, DerivedAssemblyLoadContext? dependenciesContext)
    {
        ModuleContext context;
        if (dependenciesContext is null)
            context = _defaultContext;
        else
        {
            var assemblyResolver = new LoadContextAssemblyResolver(dependenciesContext);

            foreach (var assemblyPath in _assemblyPaths)
            {
                assemblyResolver.PreSearchPaths.Add(assemblyPath);
            }

            context = new(assemblyResolver);
        }
        
        return new TransformationTransaction(transformers, context);
    }
    
    public ITransformationToken? PrepareTransformation(ITransformationTransaction transaction, string assemblyPath)
    {
        if (transaction is not TransformationTransaction transformationTransaction)
            throw new ArgumentException("Invalid transaction type", nameof(transaction));
        
        if (_tokens.TryGetValue(assemblyPath, out var token))
            return token;
        
        ModuleDefMD moduleDefinition;
        try
        {
            moduleDefinition = ModuleDefMD.Load(assemblyPath, _defaultContext);
        }
        catch (Exception)
        {
            return null;
        }
        
        var assemblyName = new AssemblyName(moduleDefinition.Assembly!.FullName).Name ?? string.Empty;
        if (transformationTransaction.Transformers.Contains(assemblyName))
            token = new TransformationToken(moduleDefinition, transformationTransaction.Transformers[assemblyName]);
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
        public ModuleDef ModuleDef { get; } = moduleDefinition;

        public void Transform(string targetPath)
        {
            var writerOptions = new ModuleWriterOptions(ModuleDef);
            foreach (var transformer in transformers)
            {
                // todo think about if bool return type is useful or not
                transformer.Transform(new(ModuleDef, writerOptions));
            }

            ModuleDef.Write(targetPath, writerOptions);
        }
    }
}

internal class TransformationTransaction : ITransformationTransaction
{
    public ModuleContext ModuleContext { get; }
    public ILookup<string, ITransformer> Transformers { get; } 

    public TransformationTransaction(IEnumerable<ITransformer> transformers, ModuleContext moduleContext)
    {
        ModuleContext = moduleContext;
        Transformers = transformers.SelectMany(x => x.AcceptedAssemblies.Select(a => (x, a)))
            .ToLookup(b => b.a.Name ?? string.Empty, b => b.x, StringComparer.OrdinalIgnoreCase);;
    }
    
    public void Dispose()
    {
    }
}
