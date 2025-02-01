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
            INamedTypeSymbol targetClass, 
            ClassToken classToken,
            Compilation compilation, 
            out HashSet<string> nameSpaces
        )
    {
        List<MethodToken> methods = [];
        nameSpaces = [];
        
        // Why isn't it returning anything???
        var availableMethods = GetValidMethods(classToken.Modifiers, sourceClass).Where(x => x.symbol is not null);
        
        // This is the default method name, that is being used when a method is not partial.
        string defaultMethodName = sourceClass.GetNameOfMapper<IMapperAttribute>(targetClass, "DefaultMapperName");
        
        // Go through each attribute, and check if it is a valid attribute.
        foreach (var attribute in availableMethods)
        {
            if (GetMethodType(attribute.symbol) is { } methodType && methodType is MethodType.None)
                continue;

            if (attribute.symbol.AttributeClass is not { } symbolAttribute)
                continue;

            string nameOfMapper = attribute.isExternal ? attribute.methodName : attribute.symbol.GetNameOfMapper<IIncludeAttribute>(targetClass, "MapperName");
            
            MethodToken generatedToken = new MethodToken(
                    GetMethodModifiers(
                            attribute.isExternal, 
                            classToken.Modifiers
                    ), 
                    methodType, 
                    nameOfMapper
                )
            {
                // There is a difference in how the mapper should type cast, it cannot use the TryParse() method if it is an expression tree for example,
                // therefor it should default to the Parse() method instead.
                IsExpressionTree = methodType is MethodType.IQueryable
            };

            var namespacesToAppend = generatedToken.HandleGenerics(sourceClass, targetClass, symbolAttribute, compilation);

            
            if (!generatedToken.Properties.Any())
                continue;
            
            nameSpaces = [..nameSpaces, ..namespacesToAppend];
            methods.Add(generatedToken);
        }
        
        // Always include the standard mapper unless the user has explicitly told not to.
        if (sourceClass.ContainsAttribute(nameof(ExcludeAttribute).AttributeAsQualifiedName())) 
            return methods;

        var standardMapperToken = CreateStandardMapper(sourceClass, classToken, defaultMethodName);

        HashSet<string> newNamespaces = standardMapperToken.HandleGenerics(sourceClass, targetClass, null, compilation);

        if (standardMapperToken.Properties.Any())
        {
            nameSpaces = [.. nameSpaces, .. newNamespaces];
        
            methods.Add(standardMapperToken);
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
                    .Where(x => x.Kind == SymbolKind.Method && x.ContainsAttribute(nameof(IncludeAttribute).AttributeAsQualifiedName()))
                    .Select(x => (x
                            .GetAttributes()
                            .FirstOrDefault(y => y.IsAttribute(nameof(IncludeAttribute).AttributeAsQualifiedName()))!, true, x.Name)
                    ) : []!)!,
            
            .. sourceSymbol
                .GetAttributes()
                .Where(x => x.IsAttribute(nameof(IncludeAttribute).AttributeAsQualifiedName()))
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

    static HashSet<string> HandleGenerics(this MethodToken generatedToken, INamedTypeSymbol sourceClass, INamedTypeSymbol targetClass, INamedTypeSymbol? attribute, Compilation compilation)
    {
        INamedTypeSymbol typeArgument = targetClass;
        
        if (attribute is { TypeArguments.IsEmpty: false } && attribute.TypeArguments[0] is { } foundTypeArgument)  
        {
            typeArgument = (foundTypeArgument as INamedTypeSymbol)!;
            generatedToken.Generic = new MethodToken.GenericType(foundTypeArgument.ToDisplayString(), foundTypeArgument.IsAbstract);
        }
        else if (sourceClass.IsAbstract)
            generatedToken.Generic = new MethodToken.GenericType(targetClass.ToDisplayString(), targetClass.IsAbstract);

        generatedToken.Properties = PropertyHelper.GetValidProperties(sourceClass, typeArgument, generatedToken.IsExpressionTree, compilation, out var newNamespaces);
        
        return newNamespaces;
    }

    static string GetNameOfMapper<T>(this INamedTypeSymbol sourceSymbol, INamedTypeSymbol targetSymbol, string propertyName)
    {
        return sourceSymbol.GetAttributeFromInterface<T>() is not { AttributeClass: not null} foundAttribute ? 
            $"MapTo{targetSymbol.Name}" : foundAttribute.GetNameOfMapper<T>(targetSymbol, propertyName);
    }

    static MethodToken CreateStandardMapper(INamedTypeSymbol sourceClass, ClassToken classToken, string defaultMethodName)
    {
        MethodToken token =
            new MethodToken(
                [!classToken.Modifiers.Contains(ModifierKind.None) ? ModifierKind.Static : ModifierKind.None],
                MethodType.Standard, 
                defaultMethodName
            );
        
        if (!classToken.SourceClass.CanCreateInstance)
            token.Generic = new MethodToken.GenericType(sourceClass.ToDisplayString(), true);
        
        return token;
    }
    
    static string GetNameOfMapper<T>(this AttributeData attribute, INamedTypeSymbol targetSymbol, string propertyName)
        => attribute.NamedArguments.FirstOrDefault(x => x.Key.Equals(propertyName)) is { Value.Value: not null } property ? 
            property.Value.Value.ToString() : $"MapTo{targetSymbol.Name}";
}