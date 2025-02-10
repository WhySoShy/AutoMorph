using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using AutoMorph.Internal.Generator;
using AutoMorph.Internal.Syntax.Providers;
using AutoMorph.Internal.Syntax.Tokens;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal;

internal static class OutputGenerator
{
    public static void Begin(SourceProductionContext context, ImmutableArray<INamedTypeSymbol> foundClasses, Compilation compilation, bool attachDebugger)
    {
        if (!Debugger.IsAttached && attachDebugger)
            Debugger.Launch();

        if (!foundClasses.Any())
            return;

        // The reason that we loop 2 times instead of combining them into one, is that the mapper makes use of its own generated mappers,
        // therefor it needs to know the names etc. of the mappers before outputting them.
        
        foreach (INamedTypeSymbol currentClass in foundClasses)
            ClassHelper.GenerateClassToken(currentClass, compilation);
        
        foreach (ClassToken token in ClassHelper.CachedClasses)
            SourceGenerator.GenerateCode(token, context);
        
        // The classes are being "reused", and I want to take advantage of this, though I am not now. Therefor I just remove the errors by clearing the Cache.
        ClassHelper.CachedClasses.Clear();
    }
}
