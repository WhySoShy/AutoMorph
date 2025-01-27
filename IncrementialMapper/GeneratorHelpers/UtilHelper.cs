using IncrementialMapper.Syntax.Tokens;
using Microsoft.CodeAnalysis;

namespace IncrementialMapper.GeneratorHelpers;

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
        if ((targetVisibility | sourceVisibility) is Accessibility.NotApplicable or Accessibility.Private or Accessibility.Protected or Accessibility.Friend)
            return false;

        return (targetVisibility | sourceVisibility) is not Accessibility.Internal || targetProperty.ContainingAssembly.Equals(sourceProperty.ContainingAssembly, SymbolEqualityComparer.Default);
    }
    
    
    /// <summary>
    /// Transforms a ISymbol into a custom ReferenceClassToken
    /// </summary>
    internal static ReferenceClassToken TransformClass(this ISymbol symbol)
    {
        return new ReferenceClassToken(symbol.Name, symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }
}