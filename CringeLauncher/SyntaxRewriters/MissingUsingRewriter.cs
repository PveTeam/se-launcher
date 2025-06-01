using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using VRage.Scripting.Rewriters;

namespace CringeLauncher.SyntaxRewriters;
internal sealed class MissingUsingRewriter : ProtoTagRewriter //use existing rewriter to prevent another iteration
{
    private readonly SemanticModel _semanticModel;
    private MissingUsingRewriter(CSharpCompilation compilation, SyntaxTree tree) : base(compilation, tree) => _semanticModel = compilation.GetSemanticModel(tree);

    public static SyntaxTree Rewrite(CSharpCompilation compilation, SyntaxTree tree)
    {
        SyntaxNode syntaxNode = new MissingUsingRewriter(compilation, tree).Visit(tree.GetRoot());
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

        Debug.WriteLine($"Missing using: {usingDirective}");
        return null;
    }
}