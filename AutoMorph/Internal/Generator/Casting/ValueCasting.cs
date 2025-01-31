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
    
    internal static CastingKind GetCastingKind(this ITypeSymbol sourceType, ITypeSymbol targetType, bool mapperIsExpressionTree)
    {
        string cacheKey = sourceType.ToDisplayString() + "->" + targetType.ToDisplayString();
        
        // Leverage caching, so we don't need to check for the types all the times.
        if (_castingCache.TryGetValue(cacheKey, out var castingKind) && !mapperIsExpressionTree && (castingKind & (CastingKind.TryParse | CastingKind.TryParseToString)) == 0)
            return castingKind;

        if (castingKind is CastingKind.None)
            cacheKey = string.Empty;
        
        if (sourceType.IsDirectCasting(targetType) || sourceType.IsImplicitCasting(targetType))
            return CastingKind.Direct.ReturnAndCacheKind(cacheKey);
        
        if (sourceType.IsExplicitCasting(targetType))
            return CastingKind.Explicit.ReturnAndCacheKind(cacheKey);
        
        if (targetType.IsStringCasting())
            return CastingKind.String.ReturnAndCacheKind(cacheKey);
            
        if (!mapperIsExpressionTree && targetType.HasTryParseMethod(sourceType) is { hasTryParse: true } tryParseMethod)
            return (tryParseMethod.useToString ? CastingKind.TryParseToString : CastingKind.TryParse).ReturnAndCacheKind(cacheKey);

        if (targetType.HasParseMethod(sourceType) is { hasParse: true} parseMethod)
            return (parseMethod.useToString ? CastingKind.ParseToString : CastingKind.Parse).ReturnAndCacheKind(cacheKey);
        
        return CastingKind.Direct;
    }
    
    static CastingKind ReturnAndCacheKind(this CastingKind kind, string cacheKey)
    {
        // Don't try to re-add the cached kind
        if (string.IsNullOrEmpty(cacheKey))
            return kind;
        
        _castingCache.Add(cacheKey, kind);
        
        return kind;
    }
    
    static bool ReturnEqualsTarget(this ImmutableArray<ISymbol> targetTypes, ITypeSymbol targetType)
        => targetTypes.OfType<IMethodSymbol>().Any(x => SymbolEqualityComparer.Default.Equals(x.ReturnType, targetType));
    
    #region Casting kinds
    
    static bool IsDirectCasting(this ITypeSymbol sourceType, ITypeSymbol targetType)
        => SymbolEqualityComparer.Default.Equals(sourceType, targetType);
    
    static bool IsImplicitCasting(this ITypeSymbol sourceType, ITypeSymbol targetType)
        => sourceType.AllInterfaces.Any(x => SymbolEqualityComparer.Default.Equals(x, targetType)) || sourceType.GetMembers(IMPLICIT_NAME).ReturnEqualsTarget(targetType);
    
    static bool IsExplicitCasting(this ITypeSymbol sourceType, ITypeSymbol targetType)
        => sourceType.GetMembers(EXPLICIT_NAME).ReturnEqualsTarget(targetType);  
    
    static bool IsStringCasting(this ITypeSymbol typeSymbol)
        => typeSymbol.SpecialType is SpecialType.System_String;
    
    static (bool hasTryParse, bool useToString) HasTryParseMethod(this ITypeSymbol typeSymbol, ITypeSymbol sourceType)
    {
        var methodType = typeSymbol.GetMembers("TryParse")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m =>
                m.DeclaredAccessibility is Accessibility.Public &&
                m is { IsStatic: true, ReturnType.SpecialType: SpecialType.System_Boolean, Parameters.Length: >= 2 } &&
                m.Parameters[0].Type.SpecialType is SpecialType.System_String || SymbolEqualityComparer.Default.Equals(m.Parameters[0].Type, sourceType) &&
                m.Parameters.Any(p => p.RefKind is RefKind.Out && SymbolEqualityComparer.Default.Equals(p.Type, typeSymbol))
            );
        
        return (methodType is not null, !SymbolEqualityComparer.Default.Equals(methodType, sourceType) || methodType.Parameters[0].Type.SpecialType is SpecialType.System_String); 
    }

    static (bool hasParse, bool useToString) HasParseMethod(this ITypeSymbol typeSymbol, ITypeSymbol sourceType)
    {
        var methodType = typeSymbol.GetMembers("Parse")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => 
                m.DeclaredAccessibility is Accessibility.Public &&
                m is { IsStatic: true, Parameters.Length: >= 1} &&
                m.Parameters.Any(p => SymbolEqualityComparer.Default.Equals(p.Type, sourceType) || p.Type.SpecialType is SpecialType.System_String)
            );

        return (methodType is not null, !SymbolEqualityComparer.Default.Equals(methodType, sourceType) || methodType.Parameters[0].Type.SpecialType is SpecialType.System_String);
    }
    
    #endregion
}