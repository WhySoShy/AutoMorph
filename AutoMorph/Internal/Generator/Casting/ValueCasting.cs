using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Types;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator.Casting;

public static class ValueCasting
{
    static readonly Dictionary<string, CastingKind> _castingCache = [];
    const string IMPLICIT_NAME = "op_Implicit";
    const string EXPLICIT_NAME = "op_Explicit";
    
    internal static CastingKind GetCastingKind(this ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        string cacheKey = sourceType.ToDisplayString() + "->" + targetType.ToDisplayString();
        
        // Leverage caching, so we don't need to check for the types all the times.
        if (_castingCache.TryGetValue(cacheKey, out var castingKind))
            return castingKind;

        if (sourceType.IsDirectCasting(targetType) || sourceType.IsImplicitCasting(targetType))
            return CastingKind.Direct.ReturnAndCacheKind(cacheKey);
        
        if (sourceType.IsExplicitCasting(targetType))
            return CastingKind.Explicit.ReturnAndCacheKind(cacheKey);
        
        if (targetType.IsStringCasting())
            return CastingKind.String.ReturnAndCacheKind(cacheKey);
            
        if (targetType.HasTryParseMethod())
            return CastingKind.TryParse.ReturnAndCacheKind(cacheKey);
        
        return CastingKind.Direct;
    }
    
    static bool IsDirectCasting(this ITypeSymbol sourceType, ITypeSymbol targetType)
        => SymbolEqualityComparer.Default.Equals(sourceType, targetType);
    
    static bool IsImplicitCasting(this ITypeSymbol sourceType, ITypeSymbol targetType)
        => sourceType.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, targetType)) || sourceType.GetMembers(IMPLICIT_NAME).ReturnEqualsTarget(targetType);
    
    static bool IsExplicitCasting(this ITypeSymbol sourceType, ITypeSymbol targetType)
        => sourceType.GetMembers(EXPLICIT_NAME).ReturnEqualsTarget(targetType);  
    
    static bool IsStringCasting(this ITypeSymbol typeSymbol)
        => typeSymbol.SpecialType is SpecialType.System_String;
    
    static bool HasTryParseMethod(this ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetMembers("TryParse")
            .OfType<IMethodSymbol>()
            .Any(m =>
                m.DeclaredAccessibility == Accessibility.Public &&
                m is { IsStatic: true, ReturnType.SpecialType: SpecialType.System_Boolean, Parameters.Length: >= 2 } &&
                m.Parameters[0].Type.SpecialType == SpecialType.System_String &&
                m.Parameters.Any(p => p.RefKind == RefKind.Out && SymbolEqualityComparer.Default.Equals(p.Type, typeSymbol))
            );
    }
    
    static CastingKind ReturnAndCacheKind(this CastingKind kind, string cacheKey)
    {
        _castingCache.Add(cacheKey, kind);

        return kind;
    }
    
    static bool ReturnEqualsTarget(this ImmutableArray<ISymbol> targetTypes, ITypeSymbol targetType)
        => targetTypes.OfType<IMethodSymbol>().Any(x => SymbolEqualityComparer.Default.Equals(x.ReturnType, targetType));
}