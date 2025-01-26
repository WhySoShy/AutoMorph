using IncrementialMapper.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using IncrementialMapper.GeneratorHelpers;
using IncrementialMapper.SyntaxProviders;

namespace IncrementialMapper;

internal static class GenerationStarter
{
    public static void Begin(SourceProductionContext context, Compilation compilation, ImmutableArray<TypeDeclarationSyntax> foundClasses, bool attachDebugger)
    {
        if (!Debugger.IsAttached && attachDebugger)
            Debugger.Launch();
        
        if (!foundClasses.Any())
            return;

        foreach (TypeDeclarationSyntax currentClassSyntax in foundClasses)
        {
            // It should only continue if it can be parsed as a INamedTypeSymbol
            if (compilation.GetSemanticModel(currentClassSyntax.SyntaxTree).GetDeclaredSymbol(currentClassSyntax) is not INamedTypeSymbol sourceSymbol)
                continue;

            ClassToken? generatedToken = ClassHelper.GenerateClassToken(sourceSymbol, currentClassSyntax);
        }

        foreach (ClassToken token in ClassHelper.CachedClasses)
            SourceGenerator.GenerateCode(token, context);
        
        // The classes are being "reused", and I want to take advantage of this, though i am not now. Therefor i just remove the errors by clearing the Cache.
        ClassHelper.CachedClasses.Clear();
    }
}
