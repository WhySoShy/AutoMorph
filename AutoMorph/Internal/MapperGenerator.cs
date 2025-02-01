using AutoMorph.Internal.Constants;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoMorph.Internal;

[Generator(LanguageNames.CSharp)]
public class MapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        ConfigureGenerator(context);
    }

    private void ConfigureGenerator(IncrementalGeneratorInitializationContext context)
    {
        // Use ForAttributeWithMetadataName instead of CreateSyntaxProvider for better performance.
        // Reference: https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md#use-forattributewithmetadataname
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            AssemblyConstants.MAPPER_FULL_QUALIFIED_METADATA_NAME,
            static (syntaxNode, _) =>
                syntaxNode is ClassDeclarationSyntax or RecordDeclarationSyntax or StructDeclarationSyntax or InterfaceDeclarationSyntax,
            static (ctx, _) => ctx.TargetSymbol as INamedTypeSymbol
        ).Where(static node => node is not null);
        
        context.RegisterSourceOutput(context.CompilationProvider.Combine(provider.Collect()), (spc, source) => OutputGenerator.Begin(spc, source!.Right!, source.Left, attachDebugger: false));
    }
}