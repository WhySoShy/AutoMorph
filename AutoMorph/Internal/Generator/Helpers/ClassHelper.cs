using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator.Helpers;

internal static class ClassHelper
{
    /// <summary>
    /// Gets the type argument within an attribute.
    /// </summary>
    /// <param name="attribute">Attribute to search in.</param>
    /// <param name="typeArgument">Found argument, this may be null if no argument was found.</param>
    /// <param name="typeArgumentIndex">Index of the argument that should be returned</param>
    /// <returns></returns>
    internal static bool GetTypeArg(this AttributeData attribute, out INamedTypeSymbol? typeArgument, int typeArgumentIndex = 0)
    {
        typeArgument = null;
        INamedTypeSymbol? attributeClass = attribute.AttributeClass;

        if (attributeClass is null || attributeClass.ContainsEmptyConstructor())
            return false;
        
        typeArgument = attributeClass.TypeArguments[typeArgumentIndex] as INamedTypeSymbol;
        
        return typeArgument is not null;
    }

    /// <summary>
    /// Checks if the instance constructor of the source is empty.
    /// </summary>
    internal static bool ContainsEmptyConstructor(this INamedTypeSymbol? source)
    {
        return source is { InstanceConstructors.IsEmpty: true };
    }
}