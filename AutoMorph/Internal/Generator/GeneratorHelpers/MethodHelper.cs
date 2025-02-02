using System.Collections.Generic;
using System.Linq;
using AutoMorph.Abstractions.Attributes;
using AutoMorph.Internal.Constants;
using AutoMorph.Internal.Syntax.Kinds;
using AutoMorph.Internal.Syntax.Tokens;
using AutoMorph.Internal.Syntax.Types;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoMorph.Internal.Generator.GeneratorHelpers;

internal static class MethodHelper
{
    internal static List<MethodToken> GetMethods(
            INamedTypeSymbol sourceClass, 
            ClassToken classToken,
            Compilation compilation, 
            out HashSet<string> nameSpaces
        )
    {
        List<MethodToken> methods = [];
        nameSpaces = [];
        
        // Go through each attribute, and check if it is a valid attribute.
        foreach (var attribute in GetValidMethods(classToken.Modifiers, sourceClass))
        {
            if (attribute.symbol is not { AttributeClass: not null } || GetMethodType(attribute.symbol) is var methodType && methodType is MethodType.None)
                continue;

            // It shouldn't continue if there is no empty constructors, because the generator does not support parameter filled constructors yet.
            if (attribute.symbol.AttributeClass is { InstanceConstructors.IsEmpty: false } ||
                attribute.symbol.AttributeClass.TypeArguments[0] is not INamedTypeSymbol targetClass)
                continue;
            
            string nameOfMapper = attribute.isExternal ? attribute.methodName : sourceClass.GetNameOfMapper<IMapperAttribute>(targetClass, "MapperName");
            
            MethodToken generatedToken = new MethodToken(
                    GetMethodModifiers(attribute.isExternal, classToken.Modifiers), 
                    methodType, 
                    nameOfMapper
                )
            {
                // There is a difference in how the mapper should type cast, it cannot use the TryParse() method if it is an expression tree for example,
                // therefor it should default to the Parse() method instead.
                IsExpressionTree = methodType is MethodType.IQueryable,
                TargetClass = targetClass.TransformClass()
            };

            var namespacesToAppend = generatedToken.HandleGenerics(sourceClass, targetClass, attribute.symbol, compilation);
            
            if (!generatedToken.Properties.Any())
                continue;
            
            nameSpaces = [..nameSpaces, ..namespacesToAppend];
            methods.Add(generatedToken);
        }

        return methods;
    }
    
    static IEnumerable<(AttributeData symbol, bool isExternal, string methodName)> GetValidMethods(List<ModifierKind> classKinds, INamedTypeSymbol sourceSymbol)
        => [
            // Get the methods that have Included attributes on them and is Partial if the class is marked as partial.
            // If the class has not been marked as partial, then it makes no sense to get methods that is maybe included.
            .. (classKinds.Any(x => x == ModifierKind.Partial) ? 
                sourceSymbol 
                    .GetMembers()
                    // We only need to iterate on the Methods, nothing else.
                    .Where(x => x.Kind == SymbolKind.Method && x.ContainsAttributeInterface(nameof(IIncludeAttribute).AttributeAsQualifiedName()))
                    .Select(x => (x
                            .GetAttributes()
                            .FirstOrDefault(y => y.IsAttributeOfInterface(nameof(IIncludeAttribute).AttributeAsQualifiedName()))!, true, x.Name)
                    ) : []!)!,
            
            .. sourceSymbol
                .GetAttributes()
                .Where(x => x.IsAttributeOfInterface(nameof(IIncludeAttribute).AttributeAsQualifiedName()))
                .Select(y => (y, false, string.Empty))!
            
            // TODO: Find a way to make this less hard-coded.
        ];

    static ModifierKind[] GetMethodModifiers(bool isExternal, List<ModifierKind> classKinds)
    {
        List<ModifierKind> modifierKinds = [];

        if (isExternal)
            modifierKinds.Add(ModifierKind.Partial);
        
        if (classKinds.Contains(ModifierKind.Static))
            modifierKinds.Add(ModifierKind.Static);
        
        return !modifierKinds.Any() ? [ModifierKind.None] : [.. modifierKinds];
    }

    static MethodType GetMethodType(AttributeData? attribute)
        // You need to increment the value with 1, else it will give an incorrect value.
        => (MethodType)(attribute?.ConstructorArguments.FirstOrDefault(x => x.Type?.Name == "MapperType").Value ?? MethodType.None)+1;

    static HashSet<string> HandleGenerics(this MethodToken generatedToken, INamedTypeSymbol sourceClass, INamedTypeSymbol targetClass, AttributeData attribute, Compilation compilation)
    {
        if (sourceClass.IsAbstract || attribute.GetValueOfNamedArgument<bool>("IsGeneric"))
            generatedToken.Generic = new MethodToken.GenericType(sourceClass.ToDisplayString());

        generatedToken.Properties = PropertyHelper.GetValidProperties(sourceClass, targetClass, generatedToken.IsExpressionTree, compilation, out var newNamespaces);
        
        return newNamespaces;
    }

    static string GetNameOfMapper<T>(this INamedTypeSymbol sourceSymbol, INamedTypeSymbol targetSymbol, string propertyName)
    {
        return sourceSymbol.GetAttributeFromInterface<T>() is not { AttributeClass: not null} foundAttribute ? 
            $"MapTo{targetSymbol.Name}" : foundAttribute.GetNameOfMapper<T>(targetSymbol, propertyName);
    }
    
    static string GetNameOfMapper<T>(this AttributeData attribute, INamedTypeSymbol targetSymbol, string propertyName)
        => attribute.NamedArguments.FirstOrDefault(x => x.Key.Equals(propertyName)) is { Value.Value: not null } property ? 
            property.Value.Value.ToString() : $"MapTo{targetSymbol.Name}";

    static T? GetValueOfNamedArgument<T>(this AttributeData attribute, string propertyName)
        => attribute.NamedArguments.FirstOrDefault(x => x.Key.Equals(propertyName)).Value.Value is T value ? value : default;
}