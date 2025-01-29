using System.Linq;
using Microsoft.CodeAnalysis;

namespace IncrementialMapper.Internal.Generator.GeneratorHelpers;

internal static class AttributeHelper
{
    internal static TReturn? GetTargetFromAttribute<TReturn, TAttribute>(ISymbol? currentClassSymbol)
        where TReturn : class
        where TAttribute : System.Attribute
    {
        if (currentClassSymbol is not null && GetAttribute<TAttribute>(currentClassSymbol) is { ConstructorArguments.Length: >= 0 } attributeData)
            return attributeData.ConstructorArguments[0].Value as TReturn;
            
        return null;
    }

    static AttributeData? GetAttribute<T>(ISymbol? currentClassSymbol)
    {
        return currentClassSymbol?.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && x.AttributeClass.Name.Equals(typeof(T).Name));
    }

    internal static bool ContainsAttribute<T>(this ISymbol? source)
    {
        return source is not null && source.GetAttributes().Any(x => x.AttributeClass?.Name == typeof(T).Name);
    }
}