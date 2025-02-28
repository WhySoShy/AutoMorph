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

        // If no key is present, then the exclusion applies globally.
        if (foundExcludeKey is null)
            return false;

        // If the key matches methodKey, apply the exclusion.
        if (foundExcludeKey == methodKey)
            return true;

        // If the key is present but does NOT match methodKey, do NOT exclude.
        return false;
    }

    /// <summary>
    /// Gets the target property from the <see cref="targetProperties"/>
    /// </summary>
    /// <returns>true if the <see cref="targetProperty"/> is not null</returns>
    internal static bool GetTargetProperty(this AttributeData? sourceAttribute, IPropertySymbol sourceProperty, List<IPropertySymbol> targetProperties, out IPropertySymbol? targetProperty)
    {
        // Ensure that the target is either set by the property name or specifically set by the user.
        string nameOfTarget = sourceAttribute?.ConstructorArguments[0].Value?.ToString() ?? sourceProperty.Name;
        
        targetProperty = targetProperties.FirstOrDefault(x => x.Name == nameOfTarget);
        targetProperties.Remove(targetProperty);
        
        return targetProperty is not null;
    }

    /// <summary>
    /// Checks whether the Get-Set methods can reach each other
    /// </summary>
    internal static bool GetSetCanReach(IPropertySymbol targetProperty, IPropertySymbol sourceProperty)
    {
        return UtilHelper.SymbolsCanReach(targetProperty, sourceProperty.GetMethod) && UtilHelper.SymbolsCanReach(sourceProperty, targetProperty.SetMethod);
    }
    
    static ImmutableArray<KeyValuePair<string, TypedConstant>> GetNamedArgumentsFromAttribute(this AttributeData? attribute)
    {
        return attribute?.NamedArguments ?? [ ];
    }
}