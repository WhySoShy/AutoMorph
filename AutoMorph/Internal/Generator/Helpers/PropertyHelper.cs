using System.Collections.Generic;
using System.Linq;
using AutoMorph.Abstractions.Attributes;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator.Helpers;

internal static class PropertyHelper
{
    internal static List<IPropertySymbol> GetProperties(this INamedTypeSymbol symbolClass, Type searchIn, string methodKey = "")
    {
        return symbolClass
            .GetMembers()
            .Where(x => x.Kind is SymbolKind.Property && (searchIn is Type.Target || x.ExcludeProperty(methodKey)))
            .Select(x => (x as IPropertySymbol)!)
            .ToList();
    }
    
    /// <summary>
    /// Checks if a property should be included determined by the methodKey.
    /// </summary>
    static bool ExcludeProperty(this ISymbol property, string? methodKey)
    {
        string? foundExcludeKey = property.GetKeyFromAttributeInterface<IExcludeAttribute>();

        // If the exclude key is present, that means the property will probably be ignored.
        // if the exclude attribute is present without any key, then the property should just get ignored by all mappers.
        return (foundExcludeKey is not null && foundExcludeKey == methodKey) || foundExcludeKey is null;
    }

    internal enum Type
    {
        Source,
        Target
    }

    internal enum KeyStatus
    {
        /// <summary>
        /// Meaning there is no key attached to the attribute.
        /// </summary>
        None,
        /// <summary>
        /// Meaning the key attached to the attribute was found on an Include.
        /// </summary>
        Valid,
        /// <summary>
        /// Meaning the key attached to the attribute was not found on any Include.
        /// </summary>
        Invalid,
    }
}