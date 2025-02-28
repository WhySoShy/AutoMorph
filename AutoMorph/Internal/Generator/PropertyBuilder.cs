using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using AutoMorph.Internal.Syntax.Tokens;
using AutoMorph.Abstractions.Attributes;
using AutoMorph.Internal.Generator.Enums;
using AutoMorph.Internal.Generator.Casting;
using AutoMorph.Internal.Generator.Helpers;

// ReSharper disable SuspiciousTypeConversion.Global

namespace AutoMorph.Internal.Generator;

public static partial class PropertyBuilder
{  
    /// <summary>
    /// Gets the properties that is valid according to the attached attributes.
    /// </summary>
    internal static HashSet<ReferencePropertyToken> GetValidProperties(
            MethodToken generatedToken,
            INamedTypeSymbol sourceClass, 
            INamedTypeSymbol targetClass, 
            Compilation compilation, 
            string? methodKey,
            out HashSet<string> newNamespaces
        )
    {
        newNamespaces = [];
        HashSet<ReferencePropertyToken> mappedProperties = [];
        
        // Get the source and target properties, according to the rules set by the attached (if any) attributes.
        if (sourceClass.GetProperties(Type.Source, methodKey) is not { Count: > 0 } sourceProperties || 
            targetClass.GetProperties(Type.Target) is not { Count: > 0 } targetProperties)
            return [];
        
        foreach (IPropertySymbol property in sourceProperties)
        {
            var foundPropertyAttribute = property.GetAttributeFromInterface<IPropertyAttribute>();
            
            // Check if the key of the property, matches the key of the declared mapper.
            // If not, it shouldn't continue. If the key is not present on the property, then it should be used as a global property, and be applied on all mappers on the source.
            if (foundPropertyAttribute.AttributeKeyMatchesMethodKey(methodKey) is KeyMatch.Invalid)
                continue;
            
            
            // Ensure that the target is found, not excluded and visible to the source property.
            if (foundPropertyAttribute.GetTargetProperty(property, targetProperties, out var foundTargetProperty) || PropertyHelper.GetSetCanReach(foundTargetProperty!, property)) 
                continue;
            
            ReferencePropertyToken newlyMappedProperty = new ReferencePropertyToken(
                    property.GetProperty(foundTargetProperty!, generatedToken.IsExpressionTree, compilation), 
                    foundTargetProperty!.GetProperty(property, generatedToken.IsExpressionTree, compilation)
                )
                {
                    NestedObject = GetNestedPropertyTokens(property, compilation, out string? newNamespace)
                };

            if (newNamespace is not null)
                newNamespaces.Add(newNamespace);

            mappedProperties.Add(newlyMappedProperty);
            // Remove it from the list, because it should only be added once.
            targetProperties.Remove(foundTargetProperty!);
        }
        
        return mappedProperties;
    }

    /// <summary>
    /// Creates a PropertyToken that is being used as data reference to the property reading from.
    /// </summary>
    static ReferencePropertyToken.Property GetProperty(this IPropertySymbol property, IPropertySymbol targetProperty, bool mapperIsExpressionTree, Compilation compilation)
    {
        return new(property.Name, property.Type.ToDisplayString(), property.Type.GetCastingKind(targetProperty.Type, mapperIsExpressionTree, compilation));
    }
}