using IncrementialMapper.Attributes;
using IncrementialMapper.Attributes.Excludes;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

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
                    GetTargetFromAttribute<INamedTypeSymbol, SGMapperAttribute>(sourceSymbol) is not INamedTypeSymbol targetSymbol ||
                    // Beacuse the generator does not support parameterless constructors, it should not try to generate on them.
                    !targetSymbol.InstanceConstructors.Any(x => x.Parameters.Length == 0)
                )
                continue;

            HashSet<ReferencePropertyToken> validatedProperties = ApplyValidProperties(sourceSymbol, targetSymbol);

            if (!validatedProperties.Any())
                continue;
            
            HashSet<MethodToken> methods = [];

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

    /// <summary>
    /// Transforms a ISymbol into a custom ReferenceClasstoken
    /// </summary>
    static ReferenceClassToken TransformClass(ISymbol symbol)
    {
        return new ReferenceClassToken(symbol.Name, symbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
    }

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
        HashSet<ReferencePropertyToken> mappedProperties = [];

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

        foreach (ISymbol property in sourceProperties)
        {
            // Ensure that the target is either set by the property name or specially set by the user.
            string? nameOfTargetProperty = GetTargetFromAttribute<string, SGPropertyAttribute>(property) ?? property.Name;
            
            ISymbol? foundTargetProperty = targetProperties.FirstOrDefault(x => x.Name == nameOfTargetProperty);
            
            if (foundTargetProperty is null || ContainsAttribute<ExcludeProperty>(foundTargetProperty) || !SymbolsCanReach(foundTargetProperty, property))
                continue;
    
            mappedProperties.Add(new ReferencePropertyToken(VisibilityKind.Public, property.Name, foundTargetProperty.Name));
            targetProperties.Remove(foundTargetProperty);
        }

        return mappedProperties;
    }

    #region Attribute related methods

    static TReturn? GetTargetFromAttribute<TReturn, TAttribute>(ISymbol currentClassSymbol, AttributeData? attributeData = null)
        where TReturn : class
        where TAttribute : System.Attribute
    {
        if (currentClassSymbol is null)
            return default;

        attributeData ??= GetAttribute<TAttribute>(currentClassSymbol);

        if (attributeData is null or { ConstructorArguments.Length: <= 0 })
            return default;

        return attributeData.ConstructorArguments[0].Value as TReturn;
    }

    static AttributeData? GetAttribute<T>(ISymbol currentClassSymbol)
    {
        return currentClassSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass.Name.Equals(typeof(T).Name));
    }

    static bool ContainsAttribute<T>(this ISymbol source)
    {
        return source.GetAttributes().Any(x => x.AttributeClass.Name == typeof(T).Name);
    }

    #endregion

    /// <summary>
    /// Checks if the symbols can see each other, determined by their visibility level.
    /// </summary>
    static bool SymbolsCanReach(ISymbol targetProperty, ISymbol sourceProperty)
    {
        Accessibility targetVisibility = targetProperty.DeclaredAccessibility;
        Accessibility sourceVisibility = sourceProperty.DeclaredAccessibility;

        // Check if either of them are non-accessible to the other.
        // There is no support for inherited classes, and dont think i will implement.
        if ((targetVisibility | sourceVisibility) is Accessibility.NotApplicable or Accessibility.Private or Accessibility.Protected or Accessibility.Friend)
            return false;

        if ((targetVisibility | sourceVisibility) is Accessibility.Internal)
            return targetProperty.ContainingAssembly.GlobalNamespace == sourceProperty.ContainingAssembly.GlobalNamespace!;

        return true;
    }
}
