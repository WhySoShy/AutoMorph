using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Abstractions.Attributes;
using IncrementialMapper.Internal.Constants;
using IncrementialMapper.Internal.Syntax.Kinds;
using IncrementialMapper.Internal.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IncrementialMapper.Internal.Generator.GeneratorHelpers;

internal static class ClassHelper
{
    /// <summary>
    /// Contains all the classes that already got a structure.
    /// </summary>
    internal static readonly HashSet<ClassToken> CachedClasses = new();
    
    internal static ClassToken? GenerateClassToken(INamedTypeSymbol? sourceSymbol)
    {
        // AttributeHelper.GetTargetFromAttribute<INamedTypeSymbol, ISGMapperAttribute>(sourceSymbol) is not { InstanceConstructors.IsEmpty: false } targetSymbol
        // Because the generator does not support parameter filled constructors, it should not try to generate on them.
    
        
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

        // Ensure that there always exists a type declaration of the source.
        // This is used to check if a class is marked as a partial class or not.
        if (PropertyHelper.GetValidProperties(sourceSymbol, targetSymbol, out HashSet<string> newNamespaces) is not { Count: > 0 } properties|| 
            sourceSymbol.GetTypeDeclaration() is not { } classDeclarationSyntax)
            return null;

        generatedToken.Properties = properties;
            
        if (!generatedToken.Modifiers.Any())
            generatedToken.Modifiers = GetModifiers(classDeclarationSyntax, sourceSymbol);
        
        if (MethodHelper.GetMethods(sourceSymbol, generatedToken.Modifiers, targetSymbol.Name) is not { Count: > 0 } methods)
            return null;
        
        // The methods generated, should be generated no matter what because the class may want more mappers generated, than the cached class has.
        generatedToken.Methods = methods;
            
        // Ensures that the generated partial class will use the same namespace as the source class. 
        generatedToken.Namespace ??= generatedToken.Modifiers.Any(x => x is ModifierKind.Partial) ? sourceSymbol.ContainingNamespace.ToDisplayString() : AssemblyConstants.DEFAULT_NAMESPACE;

        // Remove the unnecessary namespaces, we don't want to include namespaces that are the same as our own.
        generatedToken.NameSpaces = [.. newNamespaces.Where(x => x != generatedToken.Namespace)];
        
        return generatedToken;
    }
    
    static List<ModifierKind> GetModifiers(TypeDeclarationSyntax currentClass, ISymbol sourceSymbol)
    {
        List<ModifierKind> modifiers = [];

        // Force the generated class, to be created as a static class.
        if (sourceSymbol.ContainsAttribute<MarkAsStatic>())
            return [ModifierKind.Static];
        
        if (currentClass.Modifiers.Any(x => x.IsKind(SyntaxKind.PartialKeyword)))
            modifiers.Add(ModifierKind.Partial);
        
        if (!modifiers.Any(x => x == ModifierKind.Partial))
            modifiers.Add(ModifierKind.Static);
        
        return modifiers.OrderBy(x => x).ToList();
    }

    static TypeDeclarationSyntax? GetTypeDeclaration(this INamedTypeSymbol symbol)
        => symbol.DeclaringSyntaxReferences.FirstOrDefault()!.GetSyntax() as TypeDeclarationSyntax;

    static ClassToken GetCachedClass(this ClassToken token)
    {
        if (CachedClasses.FirstOrDefault(x => x.SourceClass.Equals(token.SourceClass) && x.TargetClass.Equals(token.TargetClass)) is { } cachedToken)
            return cachedToken;
        
        CachedClasses.Add(token);
        return token;
    }
}