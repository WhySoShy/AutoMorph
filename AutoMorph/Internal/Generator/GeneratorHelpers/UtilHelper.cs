using System.Collections.Generic;
using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using AutoMorph.Internal.Syntax.Kinds;
using Microsoft.CodeAnalysis.CSharp;

namespace AutoMorph.Internal.Generator.GeneratorHelpers;

internal static class UtilHelper
{
    /// <summary>
    /// Checks if the symbols can see each other, determined by their visibility level.
    /// </summary>
    internal static bool SymbolsCanReach(ISymbol targetProperty, ISymbol sourceProperty)
    {
        Accessibility targetVisibility = targetProperty.DeclaredAccessibility;
        Accessibility sourceVisibility = sourceProperty.DeclaredAccessibility;

        // Check if either of them are non-accessible to the other.
        // There is no support for inherited classes, and don't think I will implement.
        if (targetVisibility is Accessibility.NotApplicable or Accessibility.Private or Accessibility.Protected or Accessibility.Friend or Accessibility.ProtectedOrFriend ||
            sourceVisibility is Accessibility.NotApplicable or Accessibility.Private or Accessibility.Protected or Accessibility.Friend or Accessibility.ProtectedOrFriend)
            return false;

        return targetProperty.ContainingAssembly.Equals(sourceProperty.ContainingAssembly, SymbolEqualityComparer.Default);
    }

    internal static string GetCastingAsString(ReferencePropertyToken.Property targetProperty,
        ReferencePropertyToken.Property sourceProperty, string sourceReference)
        => sourceProperty.Casting switch
        {
            CastingKind.Direct => $"{sourceReference}.{sourceProperty.Name}",
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
        => new (symbol.Name, symbol.SymbolAsQualifiedName());
    
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