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
using IncrementialMapper.SyntaxProviders;
using IncrementialMapper.Utilities;
using MethodKind = IncrementialMapper.Syntax.Kinds.MethodKind;
// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
// ReSharper disable InconsistentNaming

namespace IncrementialMapper;

internal static class GenerationStarter
{
    /// <summary>
    /// Contains all the classes that already got a structure.
    /// </summary>
    private static readonly HashSet<string> _cachedClasses = new();
    private const string DEFAULT_NAMESPACE = "IncrementialMapper.Generated.Mappers";
    
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
            
            ReferenceClassToken sourceClass = TransformClass(sourceSymbol);

            if (!_cachedClasses.Add(sourceClass.FullPath))
                continue;
            
            ReferenceClassToken targetClass = TransformClass(targetSymbol);
            
            // TODO: Display a warning with a analyzer, if the class does not contain any properties.
            HashSet<ReferencePropertyToken> validatedProperties = GetValidProperties(sourceSymbol, targetSymbol);
            
            if (!validatedProperties.Any())
                continue;
            
            ModifierKind[] kinds = GetModifiers(currentClassSyntax, sourceSymbol);
            List<MethodToken> methods = GetMethods(sourceSymbol, kinds, targetSymbol.Name);
            
            if (!methods.Any())
                continue;
            
            // Ensures that the generated partial class will use the same namespace as the source class.
            string namespaceName = kinds.Any(x => x is ModifierKind.Partial) ? sourceSymbol.ContainingNamespace.ToDisplayString() : DEFAULT_NAMESPACE; 
            
            ClassToken token = new ClassToken(
                    SourceClass: sourceClass,
                    TargetClass: targetClass,
                    Namespace: namespaceName,
                    Properties: validatedProperties,
                    Methods: methods,
                    Visibility: VisibilityKind.Public,
                    Modifiers: kinds
                );

            SourceGenerator.GenerateCode(token, context);
        }   
    }

    #region Applying Methods
    
    static ModifierKind[] GetModifiers(TypeDeclarationSyntax currentClass, ISymbol sourceSymbol)
    {
        List<ModifierKind> modifiers = [];

        // Force the generated class, to be created as a static class.
        if (sourceSymbol.ContainsAttribute<MarkAsStatic>())
            return [ModifierKind.Static];
        
        if (currentClass.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            modifiers.Add(ModifierKind.Partial);
        
        if (!modifiers.Any(x => x == ModifierKind.Partial))
            modifiers.Add(ModifierKind.Static);
        
        return modifiers.OrderBy(x => x).ToArray();
    }

    static HashSet<ReferencePropertyToken> GetValidProperties(INamedTypeSymbol sourceClass, INamedTypeSymbol targetClass)
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

    static List<MethodToken> GetMethods(INamedTypeSymbol sourceSymbol, ModifierKind[] classKinds, string targetClassName)
    {
        List<MethodToken> methods = [];

        IEnumerable<(ISymbol symbol, bool isExternal, string methodName)> availableMethods = [
            // Get the methods that have Included attributes on them and is Partial if the class is marked as partial.
            // If the class has not been marked as partial, then it makes no sense to get methods that is maybe included.
            .. (classKinds.Any(x => x == ModifierKind.Partial) ? 
                sourceSymbol 
                    .GetMembers()
                    // We only need to iterate on the Methods, nothing else.
                    .Where(x => x.Kind == SymbolKind.Method && x.ContainsAttribute<IncludeLinq>() || x.ContainsAttribute<IncludeIQueryable>())
                    .Select(x => (x
                        .GetAttributes()
                        .FirstOrDefault(z => 
                            ValidAttributes.ValidIncludeAttributes
                                .Any(y => y.Key == $"{ValidAttributes.INCLUDE_ATTRIBUTE_NAMESPACE}.{z.AttributeClass!.Name}"))!
                        .AttributeClass, true, x.Name)
                    ) : []!)!,
            
            .. sourceSymbol
                .GetAttributes()
                .Where(x => 
                    ValidAttributes.ValidIncludeAttributes
                        .Any(y => y.Key == $"{ValidAttributes.INCLUDE_ATTRIBUTE_NAMESPACE}.{x.AttributeClass!.Name}"))
                .Select(y => (y.AttributeClass, false, string.Empty))!
            
            // TODO: Find a way to make this less hard-coded.
        ];

        // This is the default method name, that is being used when a method is not partial.
        string defaultMethodName = $"MapTo{targetClassName}";
        
        // Go through each attribute, and check if it is a valid attribute.
        foreach (var attribute in availableMethods)
        {
            if (attribute.symbol is null)
                continue;
            
            string fullAttributeName = attribute.symbol.ToDisplayString();
            
            // Check if the attribute is valid for the enabled attributes.
            if (ValidAttributes.ValidIncludeAttributes.FirstOrDefault(x => x.Key == fullAttributeName).Value is not { } methodKind)
                continue;

            ModifierKind[] modifiers = new ModifierKind[1];

            if (attribute.isExternal)
                modifiers[0] = ModifierKind.Partial;
            else if (classKinds.Any(x => x == ModifierKind.Static))
                modifiers[0] = ModifierKind.Static;
            else
                modifiers[0] = ModifierKind.None;
            
            methods.Add(new MethodToken(modifiers, methodKind, attribute.isExternal ? attribute.methodName : defaultMethodName));
        }
        
        // Always include the standard mapper unless the user has explicitly told not to.
        if (!sourceSymbol.ContainsAttribute<ExcludeStandard>())
            methods.Add(new MethodToken([classKinds.Any(x => x is ModifierKind.Static) ? ModifierKind.Static : ModifierKind.None], MethodKind.Standard, defaultMethodName));
        
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
