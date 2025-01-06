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
        AttachDebugger(attachDebugger);

        if (!foundClasses.Any())
            return;

        foreach (TypeDeclarationSyntax currentClassSyntax in foundClasses)
        {
            // It should only continue if it can be parsed as a INamedTypeSymbol
            if (ModelExtensions.GetDeclaredSymbol(compilation.GetSemanticModel(currentClassSyntax.SyntaxTree), currentClassSyntax) is not INamedTypeSymbol sourceSymbol)
                continue;

            // It should only continue if the target exists.
            if (GetTargetFromAttribute<INamedTypeSymbol, SGMapperAttribute>(sourceSymbol) is not INamedTypeSymbol targetSymbol)
                continue;

            // Beacuse the generator does not support parameterless constructors, it should not try to generate on them.
            if (!targetSymbol.InstanceConstructors.Any(x => x.Parameters.Length == 0))
                continue;

            ClassToken token = new ClassToken(
                    SourceClass: TransformClass(sourceSymbol),
                    TargetClass: TransformClass(targetSymbol),
                    Properties: [],
                    Methods: [],
                    Visibility: VisibilityKind.Public,
                    Modifiers: ApplyModifiers(currentClassSyntax, sourceSymbol)
                );
        }
    }

    /// <summary>
    /// Attach the debugger if it is not present and it should.
    /// </summary>
    static void AttachDebugger(bool attachDebugger)
    {
        // Enable debugging
        // But only attach it if there is no other debugger running, to ensure only 1 is running at the time.
        if (attachDebugger && !Debugger.IsAttached) 
            Debugger.Launch();
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
            string? nameOfTargetProperty = GetTargetFromAttribute<string, SGPropertyAttribute>(property);

            if (string.IsNullOrEmpty(nameOfTargetProperty))
                continue;

            string? nameofFoundTargetProperty = targetProperties.Select(x => x.Name).FirstOrDefault(x => x == nameOfTargetProperty);

            if (string.IsNullOrEmpty(nameofFoundTargetProperty))
                continue;

            mappedProperties.Add(new ReferencePropertyToken())
        }
    }

    #region Attribute related methods

    static TReturn? GetTargetFromAttribute<TReturn, TAttribute>(ISymbol currentClassSymbol, AttributeData? attributeData = null)
        where TReturn : class
        where TAttribute : System.Attribute
    {
        if (currentClassSymbol is null)
            return default;

        attributeData ??= GetAttribute<TAttribute>(currentClassSymbol);

        if (attributeData is null or { ConstructorArguments.Length: > 0 })
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
}
