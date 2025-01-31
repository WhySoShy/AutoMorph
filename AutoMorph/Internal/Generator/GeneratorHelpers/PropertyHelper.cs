using System;
using System.Collections.Generic;
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
            out HashSet<string> newNamespaces
        )
    {
        newNamespaces = [];
        HashSet<ReferencePropertyToken> mappedProperties = [];

        // Get the source and target properties, according to the rules set by the attached (if any) attributes.
        if (sourceClass.GetSourceProperties() is not { Count: > 0 } sourceProperties || targetClass.GetTargetProperties() is not { Count: > 0 } targetProperties)
            return [];
        
        foreach (IPropertySymbol property in sourceProperties)
        {
            // Ensure that the target is either set by the property name or specially set by the user.
            string nameOfTargetProperty = AttributeHelper.GetTargetFromAttribute<string, PropertyAttribute>(property) ?? property.Name;
            
            // Ensure that the target is found, not excluded and visible to the source property.
            if (targetProperties.FirstOrDefault(x => x.Name == nameOfTargetProperty) is not { } foundTargetProperty || 
                foundTargetProperty.ContainsAttribute(nameof(ExcludeAttribute).AttributeAsQualifiedName()) || 
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
    
    static List<IPropertySymbol> GetSourceProperties(this INamedTypeSymbol sourceClass) 
        => sourceClass
            .GetMembers()
            .Where(x =>
                x.Kind == SymbolKind.Property &&
                // Should never include those properties who have directly excluded themselves.
                !x.ContainsAttribute(nameof(ExcludeAttribute).AttributeAsQualifiedName())
            )
            .Select(x => (x as IPropertySymbol)!)
            .ToList() ?? [];
    
    static List<IPropertySymbol> GetTargetProperties(this INamedTypeSymbol targetClass)
        => targetClass
            .GetMembers()
            .Where(x => x.Kind == SymbolKind.Property)
            .Select(x => (x as IPropertySymbol)!)
            .ToList();

    /// <summary>
    /// Creates a PropertyToken that is being used as data reference to the property reading from.
    /// </summary>
    static ReferencePropertyToken.Property GetProperty(this IPropertySymbol property, IPropertySymbol targetProperty, bool mapperIsExpressionTree, Compilation compilation)
    {
        return new(property.Name, property.Type.ToDisplayString(), property.Type.GetCastingKind(targetProperty.Type, mapperIsExpressionTree, compilation));
    }
}