using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IncrementialMapper;

[Generator(LanguageNames.CSharp)]
public class MapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            predicate: static (syntaxNode, _) => syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax,
            transform: static (ctx, _) => ctx.Node as TypeDeclarationSyntax
        ).Where(static node => node is not null);

        var compilation = context.CompilationProvider.Combine(provider.Collect());

        // When the time is right, register the source outputter here.
        context.RegisterSourceOutput(compilation, (spc, source) => GenerationStarter.Begin(spc, source.Left, source!.Right!, attachDebugger: false));
    }
}