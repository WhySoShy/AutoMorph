using System.Linq;
using Microsoft.CodeAnalysis;

namespace IncrementialMapper.GeneratorHelpers;

internal static class AttributeHelper
{
    public static TReturn? GetTargetFromAttribute<TReturn, TAttribute>(ISymbol? currentClassSymbol, AttributeData? attributeData = null)
        where TReturn : class
        where TAttribute : System.Attribute
    {
        if (currentClassSymbol is null)
            return null;

        attributeData ??= GetAttribute<TAttribute>(currentClassSymbol);

        if (attributeData is null or { ConstructorArguments.Length: <= 0 })
            return null;

        return attributeData.ConstructorArguments[0].Value as TReturn;
    }

    public static AttributeData? GetAttribute<T>(ISymbol? currentClassSymbol)
    {
        return currentClassSymbol?.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && x.AttributeClass.Name.Equals(typeof(T).Name));
    }

    public static bool ContainsAttribute<T>(this ISymbol? source)
    {
        return source is not null && source.GetAttributes().Any(x => x.AttributeClass?.Name == typeof(T).Name);
    }
}