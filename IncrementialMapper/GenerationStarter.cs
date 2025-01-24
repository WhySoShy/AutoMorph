using IncrementialMapper.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Linq;
using IncrementialMapper.GeneratorHelpers;

namespace IncrementialMapper;

internal static class GenerationStarter
{
    public static void Begin(SourceProductionContext context, Compilation compilation, ImmutableArray<TypeDeclarationSyntax> foundClasses, bool attachDebugger)
    {
        if (!foundClasses.Any())
            return;

        foreach (TypeDeclarationSyntax currentClassSyntax in foundClasses)
        {
            // It should only continue if it can be parsed as a INamedTypeSymbol
            if (compilation.GetSemanticModel(currentClassSyntax.SyntaxTree).GetDeclaredSymbol(currentClassSyntax) is not INamedTypeSymbol sourceSymbol)
                continue;

            ClassToken? generatedToken = ClassHelper.GenerateClassToken(sourceSymbol, currentClassSyntax);

            if (generatedToken is null)
                continue;
        }   
    }
}
