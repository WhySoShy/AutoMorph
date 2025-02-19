using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMorph.Abstractions.Attributes;
using AutoMorph.Internal.Generator.Enums;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator.Helpers;

internal static class PropertyHelper
{
    /// <summary>
    /// Gets all properties from a valid type.
    /// </summary>
    /// <param name="symbol">Type that should be looked through</param>
    /// <param name="searchIn">This defines how the properties will be included / excluded, read more: <see cref="Type"/></param>
    /// <param name="methodKey"></param>
    internal static List<IPropertySymbol> GetProperties(this INamedTypeSymbol symbol, Type searchIn, string? methodKey = "")
    {
        return symbol
            .GetMembers()
            .Where(x => x.Kind is SymbolKind.Property && (searchIn is Type.Target || !x.ContainsExcludeAttribute(methodKey)))
            .Select(x => (x as IPropertySymbol)!)
            .ToList();
    }

    internal static KeyMatch AttributeKeyMatchesMethodKey(this AttributeData? attribute, string? methodKey)
    {
        if (attribute.GetNamedArgumentsFromAttribute().FirstOrDefault(x => x.Key.Equals("Key")).Value is not { Value: string key })
            return KeyMatch.Global;

        return methodKey == key ? KeyMatch.Valid : KeyMatch.Invalid;
    }
    
    /// <summary>
    /// Checks if a property should be included determined by the methodKey.
    /// </summary>
    internal static bool ContainsExcludeAttribute(this ISymbol property, string? methodKey)
    {
        string? foundExcludeKey = property.GetKeyFromAttributeInterface<IExcludeAttribute>();

        // If no key is present, the exclude applies globally (return true).
        if (foundExcludeKey is null)
            return false;

        // If the key matches methodKey, apply the exclude (return true).
        if (foundExcludeKey == methodKey)
            return true;

        // If the key is present but does NOT match methodKey, do NOT exclude (return false).
        return false;
    }

    
    static ImmutableArray<KeyValuePair<string, TypedConstant>> GetNamedArgumentsFromAttribute(this AttributeData? attribute)
    {
        return attribute?.NamedArguments ?? [ ];
    }
}