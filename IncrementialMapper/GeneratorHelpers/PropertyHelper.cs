using System;
using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Attributes;
using IncrementialMapper.Attributes.Excludes;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;
using Microsoft.CodeAnalysis;

namespace IncrementialMapper.GeneratorHelpers;

public static class PropertyHelper
{
    private static HashSet<string> _allowedCollectionInterfaces = 
    [
        "System.Collections.IEnumerable",
        "System.Collections.IList",
        "System.Collections.ICollection",
        "System.Collections.Generic.IEnumerable<T>",
        "System.Collections.Generic.ICollection<T>",
        "System.Collections.Generic.IList<T>"
    ];
    
    internal static HashSet<ReferencePropertyToken> GetValidProperties(INamedTypeSymbol sourceClass, INamedTypeSymbol targetClass)
    {
        List<IPropertySymbol> sourceProperties = sourceClass
            .GetMembers()
            .Where(x =>
                x.Kind == SymbolKind.Property &&
                // Should never include those properties who have directly excluded themselves.
                !x.ContainsAttribute<ExcludeProperty>()
            )
            .Select(x => (x as IPropertySymbol)!)
            .ToList() ?? [];

        if (!sourceProperties.Any())
            return [];

        List<IPropertySymbol> targetProperties = targetClass
            .GetMembers()
            .Where(x => x.Kind == SymbolKind.Property)
            .Select(x => (x as IPropertySymbol)!)
            .ToList();
        
        HashSet<ReferencePropertyToken> mappedProperties = [];
        
        foreach (IPropertySymbol property in sourceProperties)
        {
            // Ensure that the target is either set by the property name or specially set by the user.
            string nameOfTargetProperty = AttributeHelper.GetTargetFromAttribute<string, SGPropertyAttribute>(property) ?? property.Name;
            
            IPropertySymbol? foundTargetProperty = targetProperties.FirstOrDefault(x => x.Name == nameOfTargetProperty);
            
            if (foundTargetProperty is null || foundTargetProperty.ContainsAttribute<ExcludeProperty>() || !UtilHelper.SymbolsCanReach(foundTargetProperty, property))
                continue;

            PropertyKind propertyKind = property.Type.TypeKind is TypeKind.Class or TypeKind.Struct or TypeKind.Array && property.Type.SpecialType is SpecialType.None ? PropertyKind.Object : PropertyKind.Primitive;

            ReferencePropertyToken newlyMappedProperty = new ReferencePropertyToken(property.Name, foundTargetProperty.Name)
            {
                SourcePropertyType = propertyKind
            };
            
            // Get the properties of the object, that has been created, this should probably be created with a standalone extension method so it will be more readable.
            newlyMappedProperty.NestedObjectMapperMethod = GetNestedPropertyTokens(property, foundTargetProperty, newlyMappedProperty);

            mappedProperties.Add(newlyMappedProperty);
            // Remove it from the list, because it should only be added once.
            targetProperties.Remove(foundTargetProperty);
        }
        
        return mappedProperties;
    }

    private static string? GetNestedPropertyTokens(IPropertySymbol sourceProperty, IPropertySymbol targetProperty, ReferencePropertyToken token)
    {
        if (token.SourcePropertyType is not PropertyKind.Object)
            return null;

        INamedTypeSymbol? targetSymbolType = null;
        
        if (sourceProperty.Type.AllInterfaces.FirstOrDefault(x => _allowedCollectionInterfaces.Contains(x.OriginalDefinition.ToDisplayString()) && x.IsGenericType) is { } foundInterface)
        {
            token.SourcePropertyType = PropertyKind.Collection;

            targetSymbolType = foundInterface.TypeArguments.FirstOrDefault() as INamedTypeSymbol;
        }

        ClassToken? generatedClass = ClassHelper.GenerateClassToken(targetSymbolType, null);

        throw new NotImplementedException();
    }
}