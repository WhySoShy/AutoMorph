﻿using System.Collections.Generic;
using System.Linq;
using AutoMorph.Abstractions.Attributes;
using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Tokens;
using Microsoft.CodeAnalysis;

namespace AutoMorph.Internal.Generator.GeneratorHelpers;

internal static class ClassHelper
{
    /// <summary>
    /// Contains all the classes that already got a structure.
    /// </summary>
    internal static readonly HashSet<ClassToken> CachedClasses = new();
    
    internal static ClassToken? GenerateClassToken(INamedTypeSymbol? sourceSymbol)
    {
        if (sourceSymbol?.GetTypeParametersFromAttribute<IMapperAttribute>()[0] is not INamedTypeSymbol targetSymbol)
            return null;
        
        // Because the generator does not support parameter filled constructors, it should not try to generate on them.
        if (!targetSymbol.InstanceConstructors.Any(x => !x.Parameters.Any()))
            return null;
        
        ClassToken generatedToken = new ()
        {
            SourceClass = sourceSymbol.TransformClass(),
            TargetClass = targetSymbol.TransformClass(),
            Visibility = VisibilityKind.Public // This could technically change if the class was a partial class.
        };
        
        // Because the generator depends on its own mappers, it may try to generate the same class multiple times.
        // And to not do this, we cache the classes and retrieve them if needed.
        if (generatedToken.GetCachedClass() is { } cachedClass)
            generatedToken = cachedClass;
            
        if (!generatedToken.Modifiers.Any())
            generatedToken.Modifiers = GetModifiers(sourceSymbol);
        
        if (MethodHelper.GetMethods(sourceSymbol, targetSymbol, generatedToken.Modifiers, targetSymbol.Name, out var newNamespaces) is not { Count: > 0 } methods)
            return null;
        
        // The methods generated, should be generated no matter what because the class may want more mappers generated, than the cached class has.
        generatedToken.Methods = methods;
            
        // Ensures that the generated partial class will use the same namespace as the source class. 
        generatedToken.Namespace ??= generatedToken.Modifiers.Any(x => x is ModifierKind.Partial) ? sourceSymbol.ContainingNamespace.ToDisplayString() : AssemblyConstants.DEFAULT_NAMESPACE;
        
        // Remove the unnecessary namespaces, we don't want to include namespaces that are the same as our own.
        generatedToken.NameSpaces = [.. newNamespaces.Where(x => x != generatedToken.Namespace)];
        
        return generatedToken;
    }
    
    static List<ModifierKind> GetModifiers(INamedTypeSymbol sourceSymbol)
    {
        List<ModifierKind> modifiers = [];

        // Force the generated class, to be created as a static class.
        if (sourceSymbol.ContainsAttribute(nameof(MarkAsStatic).AttributeAsQualifiedName()))
            return [ModifierKind.Static];
        
        if (sourceSymbol.IsPartial())
            modifiers.Add(ModifierKind.Partial);
        
        if (!modifiers.Any(x => x == ModifierKind.Partial))
            modifiers.Add(ModifierKind.Static);
        
        return modifiers.OrderBy(x => x).ToList();
    }

    static ClassToken GetCachedClass(this ClassToken token)
    {
        if (CachedClasses.FirstOrDefault(x => x.SourceClass.Equals(token.SourceClass) && x.TargetClass.Equals(token.TargetClass)) is { } cachedToken)
            return cachedToken;
        
        CachedClasses.Add(token);
        return token;
    }
}