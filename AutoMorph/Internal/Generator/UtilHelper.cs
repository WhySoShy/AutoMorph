using System.Linq;
using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoMorph.Internal.Generator;

internal static class UtilHelper
{
    /// <summary>
    /// Checks if the symbols can see each other, determined by their visibility level.
    /// </summary>
    internal static bool SymbolsCanReach(ISymbol? targetProperty, ISymbol? sourceProperty)
    {
        if (targetProperty is null || sourceProperty is null)
            return false;
        
        Accessibility targetVisibility = targetProperty.DeclaredAccessibility;
        Accessibility sourceVisibility = sourceProperty.DeclaredAccessibility;

        // If either property is not accessible or has restricted visibility, return false
        if (IsRestrictedVisibility(targetVisibility) || IsRestrictedVisibility(sourceVisibility))
            return false;

        bool isInSameAssembly = SymbolEqualityComparer.Default.Equals(targetProperty.ContainingAssembly, sourceProperty.ContainingAssembly);

        // If properties are in different assemblies but are both public, they can reach each other
        if (targetVisibility is Accessibility.Public && sourceVisibility is Accessibility.Public || !isInSameAssembly && targetVisibility is Accessibility.Public && sourceVisibility is Accessibility.Public)
            return true;
        
        // If both properties are internal and in the same assembly, they can reach each other
        return isInSameAssembly && targetVisibility is Accessibility.Internal && sourceVisibility is Accessibility.Internal;
    }
    
    static bool IsRestrictedVisibility(this Accessibility visibility)
    {
        return visibility is Accessibility.NotApplicable
            or Accessibility.Private
            or Accessibility.Protected
            or Accessibility.ProtectedOrFriend;
    }


    internal static string GetCastingAsString(ReferencePropertyToken.Property targetProperty,
        ReferencePropertyToken.Property sourceProperty, string sourceReference)
        => sourceProperty.Casting switch
        {
            CastingKind.Direct or CastingKind.None => $"{sourceReference}.{sourceProperty.Name}",
            CastingKind.Explicit => $"({targetProperty.ValueType}){sourceReference}.{sourceProperty.Name}",
            CastingKind.String => $"{sourceReference}.{sourceProperty.Name}.ToString()",
            CastingKind.TryParse => $"{targetProperty.ValueType}.TryParse({sourceReference}.{sourceProperty.Name}, out {targetProperty.ValueType} entity) ? entity : default",
            CastingKind.TryParseToString => $"{targetProperty.ValueType}.TryParse({sourceReference}.{sourceProperty.Name}.ToString(), out {targetProperty.ValueType} entity) ? entity : default",
            CastingKind.Parse => $"{targetProperty.ValueType}.Parse({sourceReference}.{sourceProperty.Name})",
            CastingKind.ParseToString => $"{targetProperty.ValueType}.Parse({sourceReference}.{sourceProperty.Name}.ToString())",
        };
    
    /// <summary>
    /// Transforms a ISymbol into a custom ReferenceClassToken
    /// </summary>
    internal static ReferenceClassToken TransformClass(this ISymbol symbol)
        => new (symbol.Name, symbol.SymbolAsQualifiedName(), !symbol.IsAbstract);
    
    /// <summary>
    /// Gets the <c>.ToDisplayString()</c> containing the namespace for the symbol.
    /// </summary>
    internal static string SymbolAsQualifiedName(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    internal static string AttributeAsQualifiedName(this string nameOfAttribute)
        => AssemblyConstants.FULLY_QUALIFIED_ATTRIBUTE_NAMESPACE + "." + nameOfAttribute;
    

    internal static TypeDeclarationSyntax GetTypeDeclaration(this INamedTypeSymbol symbol)
        => (symbol.DeclaringSyntaxReferences.FirstOrDefault()!.GetSyntax() as TypeDeclarationSyntax)!;
    
    internal static bool IsPartial(this INamedTypeSymbol symbol)
        => symbol.GetTypeDeclaration().Modifiers.Any(SyntaxKind.PartialKeyword); 
}