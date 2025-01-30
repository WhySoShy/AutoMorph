using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator.GeneratorHelpers;

internal static class AttributeHelper
{
    
    internal static ImmutableArray<ITypeSymbol?> GetTypeParametersFromAttribute<TInterface>(this ISymbol? sourceSymbol)
    {
        if (sourceSymbol?.GetAttributeFromInterface<TInterface>() is not { } attribute)
            return [];

        return [..attribute.AttributeClass?.TypeArguments ?? [ ]];
    }
    
    internal static TReturn? GetTargetFromAttribute<TReturn, TAttribute>(ISymbol? currentClassSymbol)
        where TReturn : class
    {
        if (currentClassSymbol is not null && GetAttribute<TAttribute>(currentClassSymbol) is { ConstructorArguments.Length: >= 0 } attributeData)
            return attributeData.ConstructorArguments[0].Value as TReturn;
            
        return null;
    }
    
    static AttributeData? GetAttribute<T>(ISymbol? currentClassSymbol)
    {
        return currentClassSymbol?.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && x.AttributeClass.Name.Equals(typeof(T).Name));
    }

    static AttributeData? GetAttributeFromInterface<T>(this ISymbol? sourceSymbol)
    {
        return sourceSymbol?
            .GetAttributes()
            .FirstOrDefault(x => 
                (bool)x.AttributeClass?.AllInterfaces
                    .Any(y => y.ToDisplayString() == typeof(T).FullName)
                );
    }

    internal static bool ContainsAttribute(this ISymbol? source, string fullyQualifiedAttributeName)
    {
        return source is not null && source.GetAttributes().Any(x => x.AttributeClass?.ToDisplayString() == fullyQualifiedAttributeName || x.AttributeClass?.BaseType?.ToDisplayString() == fullyQualifiedAttributeName);
    }
    
    /// <summary>
    /// Checks if an attribute is of type <see cref="fullyQualifiedAttributeName"/> or its base type is of type.
    /// </summary>
    internal static bool IsAttribute(this AttributeData source, string fullyQualifiedAttributeName)
    {
        return source.AttributeClass?.ToDisplayString() == fullyQualifiedAttributeName || source.AttributeClass?.BaseType?.ToDisplayString() == fullyQualifiedAttributeName;
    }
}