using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NLog;
using System.Diagnostics;
using VRage.Scripting.Rewriters;

namespace CringeLauncher.SyntaxRewriters;
internal sealed class MissingUsingRewriter : ProtoTagRewriter //use existing rewriter to prevent another iteration
{
    private static ILogger Log = LogManager.GetCurrentClassLogger();

    private readonly SemanticModel _semanticModel;
    private readonly bool _debug;

    private MissingUsingRewriter(CSharpCompilation compilation, SyntaxTree tree, bool debug) : base(compilation, tree)
    {
        _semanticModel = compilation.GetSemanticModel(tree);
        _debug = debug;
    }

    public static SyntaxTree Rewrite(CSharpCompilation compilation, SyntaxTree tree, bool debug)
    {
        SyntaxNode syntaxNode = new MissingUsingRewriter(compilation, tree, debug).Visit(tree.GetRoot());
        return tree.WithRootAndOptions(syntaxNode, tree.Options);
    }

    public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
    {
        var visited = base.VisitUsingDirective(node);

        if (visited is not UsingDirectiveSyntax usingDirective)
            return visited;

        var symbolInfo = _semanticModel.GetSymbolInfo(node.NamespaceOrType);

        if (symbolInfo.Symbol is INamespaceOrTypeSymbol)
            return usingDirective;

        if (_debug)
            Log.Info($"Missing using: {usingDirective}");

        return null;
    }
}