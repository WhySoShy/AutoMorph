using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Syntax.Tokens;
using Microsoft.CodeAnalysis;

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
    
    
    /// <summary>
    /// Transforms a ISymbol into a custom ReferenceClassToken
    /// </summary>
    internal static ReferenceClassToken TransformClass(this ISymbol symbol)
        => new ReferenceClassToken(symbol.Name, symbol.SymbolAsQualifiedName());
    
    /// <summary>
    /// Gets the <c>.ToDisplayString()</c> containing the namespace for the symbol.
    /// </summary>
    internal static string SymbolAsQualifiedName(this ISymbol symbol)
        => symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

    internal static string AttributeAsQualifiedName(this string nameOfAttribute)
    {
        return AssemblyConstants.FULLY_QUALIFIED_ATTRIBUTE_NAMESPACE + "." + nameOfAttribute;
    }
}