using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Attributes;
using IncrementialMapper.Attributes.Includes;
using IncrementialMapper.Syntax.Kinds;
using IncrementialMapper.Syntax.Tokens;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IncrementialMapper.Generator.GeneratorHelpers;

internal static class ClassHelper
{
    private const string DEFAULT_NAMESPACE = "IncrementialMapper.Generated.Mappers";
    
    /// <summary>
    /// Contains all the classes that already got a structure.
    /// </summary>
    internal static readonly HashSet<ClassToken> CachedClasses = new();
    
    internal static ClassToken? GenerateClassToken(INamedTypeSymbol? sourceSymbol)
    {
        if (sourceSymbol is null)
            return null;
        
        // INamedTypeSymbol? targetSymbol = AttributeHelper.GetTargetFromAttribute<INamedTypeSymbol, SGMapperAttribute>(sourceSymbol);

        // Because the generator does not support parameter filled constructors, it should not try to generate on them.
        if (AttributeHelper.GetTargetFromAttribute<INamedTypeSymbol, SGMapperAttribute>(sourceSymbol) is not
            { InstanceConstructors.IsEmpty: true } targetSymbol)
            return null;
        
        // if (targetSymbol is null || !targetSymbol.InstanceConstructors.Any(x => x.Parameters.Length == 0))
        //     return null;
        
        ClassToken generatedToken = new ()
        {
            SourceClass = sourceSymbol.TransformClass(),
            TargetClass = targetSymbol.TransformClass(),
            Visibility = VisibilityKind.Public // This could technically change if the class was a partial class.
        };

        bool cachedClassWasFound = false;
        // If the source and target class already has been through this, then it should reuse the same data.
        if (CachedClasses.FirstOrDefault(x => x.SourceClass.FullPath == generatedToken.SourceClass.FullPath && x.TargetClass.FullPath == generatedToken.TargetClass.FullPath) is { } cachedToken)
        {
            generatedToken = cachedToken;
            cachedClassWasFound = true;
        }

        // Namespaces that are needed for the mapper to function.
        HashSet<string> newNamespaces = [];
        
        // TODO: Display a warning with a analyzer, if the class does not contain any properties.
        if (!generatedToken.Properties.Any())
            generatedToken.Properties = PropertyHelper.GetValidProperties(sourceSymbol, targetSymbol, out newNamespaces);

        if (!generatedToken.Properties.Any())
            return null;

        // Ensure that there always exists a type declaration of the source.
        // This is used to check if a class is marked as a partial class or not.

        if (sourceSymbol.DeclaringSyntaxReferences.FirstOrDefault()!.GetSyntax() is not TypeDeclarationSyntax classDeclarationSyntax)
            return null;
            
        if (!generatedToken.Modifiers.Any())
            generatedToken.Modifiers = GetModifiers(classDeclarationSyntax, sourceSymbol);
        
        // The methods generated, should be generated no matter what because the class may want more mappers generated, than the cached class has.
        generatedToken.Methods = MethodHelper.GetMethods(sourceSymbol, generatedToken.Modifiers, targetSymbol.Name);
            
        if (!generatedToken.Methods.Any())
            return null;
            
        // Ensures that the generated partial class will use the same namespace as the source class. 
        generatedToken.Namespace ??= generatedToken.Modifiers.Any(x => x is ModifierKind.Partial) ? sourceSymbol.ContainingNamespace.ToDisplayString() : DEFAULT_NAMESPACE;

        // Remove the unnecessary namespaces, we don't want to include namespaces that are the same as our own.
        generatedToken.NameSpaces = [.. newNamespaces.Where(x => x != generatedToken.Namespace)];
        
        if (!cachedClassWasFound)
            CachedClasses.Add(generatedToken);

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

}