﻿using IncrementialMapper.Attributes;
using IncrementialMapper.Attributes.Excludes;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using IncrementialMapper.Attributes.Includes;
using IncrementialMapper.Utilities;
using MethodKind = IncrementialMapper.Syntax.Kinds.MethodKind;

namespace IncrementialMapper;

internal static class GenerationStarter
{
    public static void Begin(SourceProductionContext context, Compilation compilation, ImmutableArray<TypeDeclarationSyntax> foundClasses, bool attachDebugger)
    {
        if (!foundClasses.Any())
            return;

        foreach (TypeDeclarationSyntax currentClassSyntax in foundClasses)
        {
            
            if (
                    // It should only continue if it can be parsed as a INamedTypeSymbol
                    ModelExtensions.GetDeclaredSymbol(compilation.GetSemanticModel(currentClassSyntax.SyntaxTree), currentClassSyntax) is not INamedTypeSymbol sourceSymbol ||
                    // It should only continue if the target exists.
                    GetTargetFromAttribute<INamedTypeSymbol, SGMapperAttribute>(sourceSymbol) is not { } targetSymbol ||
                    // Because the generator does not support parameterless constructors, it should not try to generate on them.
                    !targetSymbol.InstanceConstructors.Any(x => x.Parameters.Length == 0)
                )
                continue;
            
            HashSet<ReferencePropertyToken> validatedProperties = ApplyValidProperties(sourceSymbol, targetSymbol);
            
            if (!validatedProperties.Any())
                continue;
            
            HashSet<MethodToken> methods = ApplyMethods(sourceSymbol, currentClassSyntax, targetSymbol.Name);

            if (!methods.Any())
                continue;
            
            ClassToken token = new ClassToken(
                    SourceClass: TransformClass(sourceSymbol),
                    TargetClass: TransformClass(targetSymbol),
                    Properties: validatedProperties,
                    Methods: methods,
                    Visibility: VisibilityKind.Public,
                    Modifiers: ApplyModifiers(currentClassSyntax, sourceSymbol)
                );
        }   
    }

    #region Applying Methods
    
    static ModifierKind[] ApplyModifiers(TypeDeclarationSyntax currentClass, ISymbol sourceSymbol)
    {
        ModifierKind[] modifiers = [];

        if (currentClass.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            modifiers[0] = ModifierKind.Partial;

        if (sourceSymbol.IsStatic)
            modifiers[1] = ModifierKind.Static;

        return modifiers;
    }

    static HashSet<ReferencePropertyToken> ApplyValidProperties(INamedTypeSymbol sourceClass, INamedTypeSymbol targetClass)
    {
        List<ISymbol> sourceProperties = sourceClass
                    .GetMembers()
                    .Where(x =>
                        x.Kind == SymbolKind.Property &&
                        // Should never include those properties who have directly excluded themselves.
                        !x.ContainsAttribute<ExcludeProperty>()
                    )
                    .ToList();

        if (!sourceProperties.Any())
            return [];

        List<ISymbol> targetProperties = targetClass
                    .GetMembers()
                    .Where(x => x.Kind == SymbolKind.Property)
                    .ToList();
        
        HashSet<ReferencePropertyToken> mappedProperties = [];
        
        foreach (ISymbol property in sourceProperties)
        {
            // Ensure that the target is either set by the property name or specially set by the user.
            string nameOfTargetProperty = GetTargetFromAttribute<string, SGPropertyAttribute>(property) ?? property.Name;
            
            ISymbol? foundTargetProperty = targetProperties.FirstOrDefault(x => x.Name == nameOfTargetProperty);
            
            if (foundTargetProperty is null || ContainsAttribute<ExcludeProperty>(foundTargetProperty) || !SymbolsCanReach(foundTargetProperty, property))
                continue;
    
            mappedProperties.Add(new ReferencePropertyToken(VisibilityKind.Public, property.Name, foundTargetProperty.Name));
            // Remove it from the list, because it should only be added once.
            targetProperties.Remove(foundTargetProperty);
        }
        
        return mappedProperties;
    }

    static HashSet<MethodToken> ApplyMethods(INamedTypeSymbol sourceSymbol, TypeDeclarationSyntax currentClass, string targetClassName)
    {
        HashSet<MethodToken> methods = [];

        IEnumerable<ISymbol> availableMethods = [];
        
        // Check if the class has any of the Include attributes attached.
        if (currentClass.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            availableMethods = [
                .. sourceSymbol
                    .GetAttributes()
                    .Where(x => 
                        ValidAttributes.ValidIncludeAttributes
                            .Any(y => y.Key == $"{ValidAttributes.INCLUDE_ATTRIBUTE_NAMESPACE}.{x.AttributeClass!.Name}"))
                    .Select(y => y.AttributeClass)!
            ];
        
        // Combine the 2 collections into one, this allows us to enumerate once over the same list, to collect 
        availableMethods =
            [ .. availableMethods, 
                .. sourceSymbol
                    .GetMembers()
                    .Where(x =>
                        x.Kind == SymbolKind.Method && x.ContainsAttribute<IncludeLinq>() || x.ContainsAttribute<IncludeIQueryable>())
                    // TODO: Find a way to make this less hard-coded.
            ];
        
        // Go through each attribute, and check if it is a valid attribute.
        foreach (ISymbol attribute in availableMethods)
        {
            if (attribute is null)
                continue;
            
            string fullAttributeName = attribute.ToDisplayString();
            
            // Check if the attribute is valid for the enabled attributes.
            if (ValidAttributes.ValidIncludeAttributes.FirstOrDefault(x => x.Key == fullAttributeName).Value is not { } methodKind)
                continue;
            
            methods.Add(new MethodToken([ModifierKind.Static], methodKind));
        }
        
        return methods;
    }
    
    #endregion

    #region Attribute Methods

    static TReturn? GetTargetFromAttribute<TReturn, TAttribute>(ISymbol? currentClassSymbol, AttributeData? attributeData = null)
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

    static AttributeData? GetAttribute<T>(ISymbol? currentClassSymbol)
    {
        return currentClassSymbol?.GetAttributes().FirstOrDefault(x => x.AttributeClass is not null && x.AttributeClass.Name.Equals(typeof(T).Name));
    }

    static bool ContainsAttribute<T>(this ISymbol? source)
    {
        return source is not null && source.GetAttributes().Any(x => x.AttributeClass?.Name == typeof(T).Name);
    }

    #endregion

    #region Util Methods
    
    /// <summary>
    /// Checks if the symbols can see each other, determined by their visibility level.
    /// </summary>
    static bool SymbolsCanReach(ISymbol targetProperty, ISymbol sourceProperty)
    {
        Accessibility targetVisibility = targetProperty.DeclaredAccessibility;
        Accessibility sourceVisibility = sourceProperty.DeclaredAccessibility;

        // Check if either of them are non-accessible to the other.
        // There is no support for inherited classes, and don't think I will implement.
        if ((targetVisibility | sourceVisibility) is Accessibility.NotApplicable or Accessibility.Private or Accessibility.Protected or Accessibility.Friend)
            return false;

        return (targetVisibility | sourceVisibility) is not Accessibility.Internal || targetProperty.ContainingAssembly.Equals(sourceProperty.ContainingAssembly, SymbolEqualityComparer.Default);
    }
    
    
    /// <summary>
    /// Transforms a ISymbol into a custom ReferenceClassToken
    /// </summary>
    static ReferenceClassToken TransformClass(ISymbol symbol)
    {
        return new ReferenceClassToken(symbol.Name, symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }
    
    #endregion
}
