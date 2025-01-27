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
    public static void Begin(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> foundClasses, bool attachDebugger)
    {
        if (!Debugger.IsAttached && attachDebugger)
            Debugger.Launch();

        if (!foundClasses.Any())
            return;

        foreach (INamedTypeSymbol currentClass in foundClasses)
            ClassHelper.GenerateClassToken(currentClass);
        
        foreach (ClassToken token in ClassHelper.CachedClasses)
            SourceGenerator.GenerateCode(token, context);
        
        // The classes are being "reused", and I want to take advantage of this, though I am not now. Therefor I just remove the errors by clearing the Cache.
        ClassHelper.CachedClasses.Clear();
    }
}
