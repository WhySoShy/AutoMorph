﻿using System.Collections.Generic;
using System.Linq;
using IncrementialMapper.Abstractions.Attributes;
using IncrementialMapper.Internal.Syntax.Kinds;
using IncrementialMapper.Internal.Syntax.Tokens;
using IncrementialMapper.Internal.Syntax.Types;
using Microsoft.CodeAnalysis;

namespace IncrementialMapper.Internal.Generator.GeneratorHelpers;

internal static class MethodHelper
{
    internal static List<MethodToken> GetMethods(INamedTypeSymbol sourceSymbol, INamedTypeSymbol targetSymbol, List<ModifierKind> classKinds, string targetClassName, out HashSet<string> nameSpaces)
    {
        List<MethodToken> methods = [];
        nameSpaces = [];
        
        // Why isn't it returning anything???
        var availableMethods = GetValidMethods(classKinds, sourceSymbol).Where(x => x.symbol is not null);
        
        // This is the default method name, that is being used when a method is not partial.
        string defaultMethodName = $"MapTo{targetClassName}";
        
        // Go through each attribute, and check if it is a valid attribute.
        foreach (var attribute in availableMethods)
        {
            if (GetMethodType(attribute.symbol) is { } methodType && methodType is MethodType.None)
                continue;
            
            MethodToken generatedToken = new MethodToken(GetMethodModifiers(attribute.isExternal, classKinds), methodType,
                attribute.isExternal ? attribute.methodName : $"MapTo{targetClassName}");
            
            nameSpaces = [..nameSpaces, ..generatedToken.HandleGenerics(sourceSymbol, targetSymbol, attribute.symbol.AttributeClass)];
            
            methods.Add(generatedToken);
        }
        
        // Always include the standard mapper unless the user has explicitly told not to.
        if (!sourceSymbol.ContainsAttribute(nameof(Exclude).AttributeAsQualifiedName()))
        {
            MethodToken token =
                new MethodToken([classKinds.Contains(ModifierKind.None) ? ModifierKind.Static : ModifierKind.None],
                    MethodType.Standard, defaultMethodName);

            nameSpaces = [..nameSpaces, ..token.HandleGenerics(sourceSymbol, targetSymbol, null)];
            
            methods.Add(token);
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
                    .Where(x => x.Kind == SymbolKind.Method && x.ContainsAttribute(nameof(Include).AttributeAsQualifiedName()))
                    .Select(x => (x
                            .GetAttributes()
                            .FirstOrDefault(y => y.IsAttribute(nameof(Include).AttributeAsQualifiedName()))!, true, x.Name)
                    ) : []!)!,
            
            .. sourceSymbol
                .GetAttributes()
                .Where(x => x.IsAttribute(nameof(Include).AttributeAsQualifiedName()))
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

    static HashSet<string> HandleGenerics(this MethodToken generatedToken, INamedTypeSymbol sourceSymbol, INamedTypeSymbol targetSymbol, INamedTypeSymbol? attribute)
    {
        INamedTypeSymbol? typeArgument = null;
        // Check if the Attribute contains any type parameters, so we can generate generic mappers for that mapper type instead.
        if (attribute is not null && attribute.TypeArguments.Any() && attribute.TypeArguments.ElementAt(0) is { } extractedTypeArgument)
        {
            typeArgument = extractedTypeArgument as INamedTypeSymbol;
            generatedToken.IsGeneric = true;
            generatedToken.GenericTypeName = extractedTypeArgument.ToDisplayString();
        }

        generatedToken.Properties = PropertyHelper.GetValidProperties(sourceSymbol, typeArgument ?? targetSymbol, out var newNamespaces);
        
        return newNamespaces;
    }
}