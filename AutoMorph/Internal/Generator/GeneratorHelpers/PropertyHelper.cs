using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoMorph.Abstractions.Attributes;
using AutoMorph.Internal.Generator.Casting;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Tokens;
using AutoMorph.Internal.Syntax.Types;
using Microsoft.CodeAnalysis;

// ReSharper disable SuspiciousTypeConversion.Global

namespace AutoMorph.Internal.Generator.GeneratorHelpers;

public static partial class PropertyHelper
{  
    /// <summary>
    /// Gets the properties that is valid according to the attached attributes.
    /// </summary>
    internal static HashSet<ReferencePropertyToken> GetValidProperties(
            INamedTypeSymbol sourceClass, 
            INamedTypeSymbol targetClass, 
            bool mapperIsExpressionTree, 
            Compilation compilation, 
            string? methodKey,
            out HashSet<string> newNamespaces
        )
    {
        newNamespaces = [];
        HashSet<ReferencePropertyToken> mappedProperties = [];

        // Get the source and target properties, according to the rules set by the attached (if any) attributes.
        if (sourceClass.GetSourceProperties(methodKey) is not { Count: > 0 } sourceProperties || targetClass.GetTargetProperties() is not { Count: > 0 } targetProperties)
            return [];
        
        foreach (IPropertySymbol property in sourceProperties)
        {
            var foundPropertyAttribute = property.GetAttributeFromInterface<IPropertyAttribute>();
            var properties = foundPropertyAttribute?.NamedArguments ?? [ ];
            
            // Check if the key of the property, matches the key of the declared mapper.
            // If not, it shouldn't continue. If the key is not present on the property, then it should be used as a global property, and be applied on all mappers on the source.
            if (properties.FirstOrDefault(x => x.Key.Equals("Key")).Value is { Value: string key } && methodKey != key)
                continue;
            
            // Ensure that the target is either set by the property name or specially set by the user.
            string nameOfTargetProperty = foundPropertyAttribute?.ConstructorArguments[0].Value as string ?? property.Name;
            
            // Ensure that the target is found, not excluded and visible to the source property.
            if (targetProperties.FirstOrDefault(x => x.Name == nameOfTargetProperty) is not { } foundTargetProperty || 
                !UtilHelper.SymbolsCanReach(foundTargetProperty, property.GetMethod!) || !UtilHelper.SymbolsCanReach(property, foundTargetProperty.SetMethod!)) 
                continue;
            
            ReferencePropertyToken newlyMappedProperty = new ReferencePropertyToken(property.GetProperty(foundTargetProperty, mapperIsExpressionTree, compilation), foundTargetProperty.GetProperty(property, mapperIsExpressionTree, compilation))
                {
                    NestedObject = GetNestedPropertyTokens(property, compilation, out string? newNamespace)
                };

            if (newNamespace is not null)
                newNamespaces.Add(newNamespace);

            mappedProperties.Add(newlyMappedProperty);
            // Remove it from the list, because it should only be added once.
            targetProperties.Remove(foundTargetProperty);
        }
        
        return mappedProperties;
    }
    
    static List<IPropertySymbol> GetSourceProperties(this INamedTypeSymbol sourceClass, string? methodKey) 
        => sourceClass
            .GetMembers()
            .Where(x => x.Kind is SymbolKind.Property && x.ExcludeProperty(methodKey))
            .Select(x => (x as IPropertySymbol)!)
            .ToList();
    
    static List<IPropertySymbol> GetTargetProperties(this INamedTypeSymbol targetClass)
        => targetClass
            .GetMembers()
            .Where(x => x.Kind is SymbolKind.Property)
            .Select(x => (x as IPropertySymbol)!)
            .ToList();

    /// <summary>
    /// Creates a PropertyToken that is being used as data reference to the property reading from.
    /// </summary>
    static ReferencePropertyToken.Property GetProperty(this IPropertySymbol property, IPropertySymbol targetProperty, bool mapperIsExpressionTree, Compilation compilation)
    {
        return new(property.Name, property.Type.ToDisplayString(), property.Type.GetCastingKind(targetProperty.Type, mapperIsExpressionTree, compilation));
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
}