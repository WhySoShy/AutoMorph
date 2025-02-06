using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator.GeneratorHelpers;

internal static class AttributeHelper
{
    internal static AttributeData? GetAttributeFromInterface<T>(this ISymbol? sourceSymbol)
    {
        return sourceSymbol?
            .GetAttributes()
            .FirstOrDefault(x => x?.AttributeClass is not null && 
                                 x.AttributeClass.AllInterfaces.Any(y => y.ToDisplayString() == typeof(T).FullName)
            );
    }
    
    internal static T? GetValueOfNamedArgument<T>(this AttributeData attribute, string argumentName)
        => attribute.NamedArguments.FirstOrDefault(x => x.Key.Equals(argumentName)).Value.Value is T value ? value : default;
    
    internal static bool IsAttributeOfInterface<T>(this AttributeData source)
    {
        return (bool)source.AttributeClass?.AllInterfaces.Any(x => x.ToDisplayString() == typeof(T).FullName);
    }

    internal static bool ContainsAttributeInterface<T>(this ISymbol? source)
    {
        return (bool)source?.GetAttributes().Any(x => x.IsAttributeOfInterface<T>());
    }

    /// <summary>
    /// Gets the attached attribute of type <see cref="T"/>
    /// </summary>
    /// <typeparam name="T">Interface of the attribute</typeparam>
    internal static string? GetKeyFromAttributeInterface<T>(this ISymbol symbol)
    {
        return symbol.GetAttributeFromInterface<T>()?.NamedArguments.FirstOrDefault(x => x.Key.Equals("Key")).Value.Value as string;
    }
}